using Vortice.Direct3D;
using Vortice.Direct3D11;

namespace MyGame.Bindable
{
    internal class Topology : IBindable
    {
        private readonly PrimitiveTopology _topology;

        public Topology(PrimitiveTopology topology)
        {
            _topology = topology;
        }

        public void Bind(ID3D11DeviceContext context)
        {
            context.IASetPrimitiveTopology(_topology);
        }
    }
}
