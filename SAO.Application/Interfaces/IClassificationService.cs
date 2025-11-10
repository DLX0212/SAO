using SAO.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SAO.Application.Interfaces
{
    public interface IClassificationService
    {
        /// <summary>
        /// Clasifica una opinión y calcula su puntaje
        /// </summary>
        void ProcessOpinion(Opinion opinion);
    }
}
