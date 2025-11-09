using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SAO.Domain.Repository
{
    public interface IClassificationService
    {
        string ClassifyOpinion(string comentario, int? rating = null);
        decimal CalculateSatisfactionScore(string clasificacion, int? rating = null);
    }
}
