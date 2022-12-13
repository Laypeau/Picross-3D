using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Menu_LayoutGrid : LayoutGroup
{
	public FitType fitType;
	public enum FitType
	{
		Uniform = 0,
		Width = 1,
		Height = 2,
		FixedRows = 3,
		FixedColumns = 4
	}
	public int rows; //getters and setters: public get, protected set
	public int columns;
	public Vector2 cellSize;
	public Vector2 cellSpacing;
	public bool fitSizeX;
	public bool fitSizeY;
	private float scrollAmount;

	protected override void Start()
	{
		base.Start();
	}

	public override void CalculateLayoutInputHorizontal()
	{
		base.CalculateLayoutInputHorizontal();

		switch (fitType)
		{
			case FitType.Uniform:
				fitSizeX = true;
				fitSizeY = true;
				rows = Mathf.CeilToInt(Mathf.Sqrt(transform.childCount));
				columns = Mathf.CeilToInt(Mathf.Sqrt(transform.childCount));
				break;

			case FitType.Width:
				fitSizeX = true;
				fitSizeY = true;
				rows = Mathf.CeilToInt(Mathf.Sqrt(transform.childCount));
				columns = Mathf.CeilToInt(transform.childCount / (float)rows);
				break;

			case FitType.Height:
				fitSizeX = true;
				fitSizeY = true;
				columns = Mathf.CeilToInt(Mathf.Sqrt(transform.childCount));
				rows = Mathf.CeilToInt(transform.childCount / (float)columns);	
				break;

			case FitType.FixedRows:
				rows = Mathf.CeilToInt(Mathf.Sqrt(transform.childCount));
				columns = Mathf.CeilToInt(transform.childCount / (float)rows);
				break;

			case FitType.FixedColumns:

				rows = Mathf.CeilToInt(Mathf.Sqrt(transform.childCount));
				columns = Mathf.CeilToInt(transform.childCount / (float)rows);
				break;

			default:
				fitSizeX = true;
				fitSizeY = true;
				rows = Mathf.CeilToInt(Mathf.Sqrt(transform.childCount));
				columns = Mathf.CeilToInt(Mathf.Sqrt(transform.childCount));
				break;
		}

		float cellWidth = ((rectTransform.rect.width - padding.left - padding.right - (cellSpacing.x * (columns - 1f))) / (float)columns);
		float cellHeight = ((rectTransform.rect.height - padding.top - padding.bottom - (cellSpacing.y * (rows - 1f))) / (float)rows);

		cellSize.x = fitSizeX ? cellWidth : cellSize.x;
		cellSize.y = fitSizeY ? cellWidth : cellSize.y;

		int rowCount = 0;
		int columnCount = 0;
		for (int i = 0; i < transform.childCount; i++)
		{
			columnCount = i % columns;
			rowCount = i / columns;

			RectTransform child = rectChildren[i];

			float xPos = (cellSize.x * columnCount) + (cellSpacing.x * columnCount) + padding.left;
			float yPos = (cellSize.y * rowCount) + (cellSpacing.y * rowCount) + padding.top;

			SetChildAlongAxis(child, 0, xPos, cellSize.x);
			SetChildAlongAxis(child, 1, yPos, cellSize.y);			
		}
	}

	public override void CalculateLayoutInputVertical()
	{

	}

	public override void SetLayoutHorizontal()
	{

	}

	public override void SetLayoutVertical()
	{

	}
}
