using MediatR;

namespace Segmentation.Application.Commands.ConseillerProfil
{
    public class SaveConseillersProfilsCommand : IRequest<int>
    {
        public List<SaveConseillerProfilItem> Items { get; set; } = new();
    }

    public class SaveConseillerProfilItem
    {
        public string LigneMetier { get; set; } = "";
        public string Profil { get; set; } = "";
        public double PartTempsCommercialPct { get; set; }
    }
}
