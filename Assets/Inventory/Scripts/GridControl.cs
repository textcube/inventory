using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Grid UI.
/// </summary>
[ExecuteInEditMode]
public class GridControl : MonoBehaviour {
	public delegate void OnReposition ();
	public enum Direction {
		Down,
		Up,
	}

	public int columns = 3;
	public Direction direction = Direction.Down;
	public Vector2 padding = Vector2.zero;
	public bool sorted = false;
	public bool hideInactive = true;
	public bool repositionNow = false;
	public bool keepWithinPanel = false;
	public OnReposition onReposition;

	UIPanel mPanel;
	UIDraggablePanel mDrag;
	bool mStarted = false;
	List<Transform> mChildren = new List<Transform>();
	
	// array sort by name
	static public int SortByName (Transform a, Transform b) { return string.Compare(a.name, b.name); }
	
	// for checking fit position
	public bool[,] grid;
	// item size list
	Vector2[] sizeList;
	// item position list
	Vector2[] posList;
	int cols;
	int rows;
	public float cellWidth = 50;
	public float cellHeight = 50;
	
	// put children item list in array
	public List<Transform> children {
		get {
			if (mChildren.Count == 0) {
				Transform myTrans = transform;
				mChildren.Clear();
				for (int i = 0; i < myTrans.childCount; ++i) {
					Transform child = myTrans.GetChild(i);
					if (child && (!hideInactive || child.gameObject.active)) mChildren.Add(child);
				}
				if (sorted) mChildren.Sort(SortByName);
			}
			return mChildren;
		}
	}
	
	// check fit position
	public bool CheckGridPosition(int x, int y, int idx){
		bool ok = true;
		int r = (int)sizeList[idx].x; //2
		int c = (int)sizeList[idx].y; //1
		for (int y1=0; y1<c; y1++) { // originally, c was r and r was c, but this was wrong thought because of index overflow.
			for (int x1=0; x1<r; x1++) {
				if (grid[x+x1, y+y1]) {
					ok = false;
					break;
				}
			}
			if (!ok) break;
		}
		return ok;
	}
	
	// set grid position
	public void SetGridPosition(int idx){
		bool ok = true;
		for (int y=0; y<rows; y++) {
			for (int x=0; x<cols; x++) {
				if (((x+sizeList[idx].x-1)<columns) && CheckGridPosition(x, y, idx)) {
					posList[idx] = new Vector2(x, y);
					ok = false;
					break;
				}
			}
			if (!ok) break;
		}
		for (int y=0; y<sizeList[idx].y; y++) {
			for (int x=0; x<sizeList[idx].x; x++) {
				int x1 = (int)(posList[idx].x + x);
				int y1 = (int)(posList[idx].y + y);
				grid[x1, y1] = true;
			}
		}
	}
	
	// reposition with item size
	void RepositionVariableSize (List<Transform> children) {
		cols = columns;
		rows = children.Count;
		if (rows < columns) rows = columns;
		
		sizeList = new Vector2[children.Count];
		posList = new Vector2[children.Count];
		grid = new bool[cols, rows];
		for (int y=0; y<rows; y++) {
			for (int x=0; x<cols; x++) {
				grid[x, y] = false;
			}
		}

		for (int i = 0; i<children.Count; i++) {
			Transform t = children[i];
			Bounds b = NGUIMath.CalculateRelativeWidgetBounds(t);
			sizeList[i] = new Vector2( Mathf.Floor( Mathf.Ceil(b.size.x)/cellWidth ), Mathf.Floor( Mathf.Ceil(b.size.y)/cellHeight ) );
			SetGridPosition(i);
			float cWidth = cellWidth + padding.x;
			float cHeight = cellHeight + padding.y;
			
			Vector3 pos = new Vector3(posList[i].x*cWidth + sizeList[i].x*cWidth/2 - columns*cWidth/2 - padding.x, -cHeight*posList[i].y - sizeList[i].y*cHeight/2 - padding.y, -0.1f);
			t.localPosition = pos;	
		}
	}
	
	// delay repostion
	IEnumerator DelayReposition(float delayTime) {
		yield return new WaitForSeconds(delayTime);
		Reposition();
	}
	
	// delete grid item
	public void DeleteItem(GameItem item){
		Destroy(item.gameObject);
		StartCoroutine( DelayReposition(0.1f) );
	}
	
	// reposition
	public void Reposition () {
		if (mStarted) {
			Transform myTrans = transform;
			mChildren.Clear();
			List<Transform> ch = children;
			if (ch.Count > 0) RepositionVariableSize(ch);

			if (mDrag != null) {
				mDrag.UpdateScrollbars(true);
				mDrag.RestrictWithinBounds(true);
			} else if (mPanel != null) {
				mPanel.ConstrainTargetToBounds(myTrans, true);
			}
			if (onReposition != null) onReposition();
		} else repositionNow = true;
	}
	
	// init default value
	void Start () {
		mStarted = true;

		if (keepWithinPanel) {
			mPanel = NGUITools.FindInParents<UIPanel>(gameObject);
			mDrag = NGUITools.FindInParents<UIDraggablePanel>(gameObject);
		}
		Reposition();
	}

	// reposition when checkbox is on
	void LateUpdate () {
		if (repositionNow) {
			repositionNow = false;
			Reposition();
		}
	}
}
