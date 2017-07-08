using UnityEngine;
using FairyGUI;
using DG.Tweening;

public class TreeViewMain : MonoBehaviour
{
	GComponent _mainView;
	TreeView _treeView;
	string _folderURL1;
	string _folderURL2;
	string _fileURL;

	void Awake()
	{
		Application.targetFrameRate = 60;
		Stage.inst.onKeyDown.Add(OnKeyDown);
	}

	void Start()
	{
		_mainView = this.GetComponent<UIPanel>().ui;

		_folderURL1 = "ui://TreeView/folder_closed";
		_folderURL2 = "ui://TreeView/folder_opened";
		_fileURL = "ui://TreeView/file";

		_treeView = new TreeView(_mainView.GetChild("tree").asList);
		_treeView.onClickNode.Add(__clickNode);
		_treeView.treeNodeRender = RenderTreeNode;

		TreeNode topNode = new TreeNode(true);
		topNode.data = "I'm a top node";
		_treeView.root.AddChild(topNode);
		for (int i = 0; i < 5; i++)
		{
			TreeNode node = new TreeNode(false);
			node.data = "Hello " + i;
			topNode.AddChild(node);
		}

		TreeNode aFolderNode = new TreeNode(true);
		aFolderNode.data = "A folder node";
		topNode.AddChild(aFolderNode);
		for (int i = 0; i < 5; i++)
		{
			TreeNode node = new TreeNode(false);
			node.data = "Good " + i;
			aFolderNode.AddChild(node);
		}

		for (int i = 0; i < 3; i++)
		{
			TreeNode node = new TreeNode(false);
			node.data = "World " + i;
			topNode.AddChild(node);
		}

		TreeNode anotherTopNode = new TreeNode(false);
		anotherTopNode.data = new string[] { "I'm a top node too", "ui://TreeView/heart" };
		_treeView.root.AddChild(anotherTopNode);
	}

	void RenderTreeNode(TreeNode node)
	{
		GButton btn = (GButton)node.cell;
		if (node.isFolder)
		{
			if (node.expanded)
				btn.icon = _folderURL2;
			else
				btn.icon = _folderURL1;
			btn.title = (string)node.data;
		}
		else if (node.data is string[])
		{
			btn.icon = ((string[])node.data)[1];
			btn.title = ((string[])node.data)[0];
		}
		else
		{
			btn.icon = _fileURL;
			btn.title = (string)node.data;
		}
	}

	void __clickNode(EventContext context)
	{
		TreeNode node = (TreeNode)context.data;
		if (node.isFolder /* && context.inputEvent.isDoubleClick */)
			node.expanded = !node.expanded;
	}

	void OnKeyDown(EventContext context)
	{
		if (context.inputEvent.keyCode == KeyCode.Escape)
		{
			Application.Quit();
		}
	}
}