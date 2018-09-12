using System.Collections.Generic;
using Tests.Nodes;

public class SelectedNode
{
    private Dictionary<string, PlayQ.UITestTools.RuntimeTestRunnerWindow.TestInfoData> testsState;

    private Node node;
    public Node Node
    {
        get
        {
            return node;
        }
        set
        {
            if (node != value)
            {
                node = value;

                UpdateLogs();
            }
        }
    }

    public bool IsSelected(Node node)
    {
        return this.node == node;
    }

    private Node previousNode;
    public bool WasAnotherNodeSelected { get { return node != previousNode; } }

    public void MemorizePreviousNode()
    {
        previousNode = node;
    }

    public List<string> logs = new List<string>();

    public SelectedNode(Dictionary<string, PlayQ.UITestTools.RuntimeTestRunnerWindow.TestInfoData> testsState)
    {
        this.testsState = testsState;
    }

    private void UpdateLogs()
    {
        logs.Clear();

        if (node is MethodNode && testsState.ContainsKey(node.FullName))
            logs.AddRange(testsState[node.FullName].Logs.ToArray());
    }
}
