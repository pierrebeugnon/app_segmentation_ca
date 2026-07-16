using MediatR;

namespace Segmentation.Application.Commands.ParametresGeneraux
{
    public class SaveParametresGenerauxCommand : IRequest<int>
    {
        public double HeuresParSemaine { get; set; }
        public double NbSemainesParAn { get; set; }
    }
}
