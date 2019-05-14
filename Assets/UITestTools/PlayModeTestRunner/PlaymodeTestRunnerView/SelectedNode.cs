using Tests.Nodes;

public class SelectedNode
{
    public SelectedNode(Node node)
    {
        this.node = node;
    }
    private Node node;
    public Node Node
    {
        get
        {
            return node;
        }
    }

    public void UpdateSelectedNode(Node node)
    {
        this.node = node;
    }

    public bool IsGivenNodeSelected(Node node)
    {
        return this.node == node;
    }
}
