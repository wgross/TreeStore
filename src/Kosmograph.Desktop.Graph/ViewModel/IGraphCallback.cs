namespace Kosmograph.Desktop.Graph.ViewModel
{
    public interface IGraphCallback
    {
        void Add(VertexViewModel vertex);

        void Remove(VertexViewModel vertex);

        void Add(EdgeViewModel edge);

        void Remove(EdgeViewModel edge);
    }
}