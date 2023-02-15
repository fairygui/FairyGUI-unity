using UnityEngine;
using FairyGUI;

public class TreeViewMain : MonoBehaviour
{
    GComponent _mainView;
    GTree _tree1;
    GTree _tree2;
    string _fileURL;

    void Awake()
    {
        Application.targetFrameRate = 60;
        Stage.inst.onKeyDown.Add(OnKeyDown);
    }

    void Start()
    {
        _mainView = this.GetComponent<UIPanel>().ui;

        _fileURL = "ui://TreeView/file";

        _tree1 = _mainView.GetChild("tree").asTree;
        _tree1.onClickItem.Add(__clickNode);
        _tree2 = _mainView.GetChild("tree2").asTree;
        _tree2.onClickItem.Add(__clickNode);
        _tree2.treeNodeRender = RenderTreeNode;

        GTreeNode topNode = new GTreeNode(true);
        topNode.data = "I'm a top node";
        _tree2.rootNode.AddChild(topNode);
        for (int i = 0; i < 5; i++)
        {
            GTreeNode node = new GTreeNode(false);
            node.data = "Hello " + i;
            topNode.AddChild(node);
        }

        GTreeNode aFolderNode = new GTreeNode(true);
        aFolderNode.data = "A folder node";
        topNode.AddChild(aFolderNode);
        for (int i = 0; i < 5; i++)
        {
            GTreeNode node = new GTreeNode(false);
            node.data = "Good " + i;
            aFolderNode.AddChild(node);
        }

        for (int i = 0; i < 3; i++)
        {
            GTreeNode node = new GTreeNode(false);
            node.data = "World " + i;
            topNode.AddChild(node);
        }

        GTreeNode anotherTopNode = new GTreeNode(false);
        anotherTopNode.data = new string[] { "I'm a top node too", "ui://TreeView/heart" };
        _tree2.rootNode.AddChild(anotherTopNode);
    }

    void RenderTreeNode(GTreeNode node, GComponent obj)
    {
        if (node.isFolder)
        {
            obj.text = (string)node.data;
        }
        else if (node.data is string[])
        {
            obj.icon = ((string[])node.data)[1];
            obj.text = ((string[])node.data)[0];
        }
        else
        {
            obj.icon = _fileURL;
            obj.text = (string)node.data;
        }
    }

    void __clickNode(EventContext context)
    {
        GTreeNode node = ((GObject)context.data).treeNode;
        Debug.Log(node.text);
    }

    void OnKeyDown(EventContext context)
    {
        if (context.inputEvent.keyCode == KeyCode.Escape)
        {
            Application.Quit();
        }
    }
}