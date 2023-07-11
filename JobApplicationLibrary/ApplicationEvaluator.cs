using JobApplicationLibrary.Models;
using JobApplicationLibrary.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JobApplicationLibrary
{
    public class ApplicationEvaluator: IDisposable
    {
        private const int minAge = 18;
        private const int autoAcceptedYearsOfExperience = 10;
        private List<string> TechStackList = new() { "C#", "RabbitMQ", "Docker", "Microservice", "VisualStudio"};
        private IIdentityValidator _iIdentityValidator;
        private bool disposedValue;

        public ApplicationEvaluator(IIdentityValidator iIdentityValidator)
        {
            _iIdentityValidator = iIdentityValidator;
        }


        public ApplicatonResult Evaluate(JobApplication form)
        {
            if(form.Applicant == null)
            {
                throw new ArgumentNullException(nameof(form.Applicant));
            }
            
            if(form.Applicant.Age < minAge)
            {
                return ApplicatonResult.AutoReject;
            }

            _iIdentityValidator.ValidationMode = form.Applicant.Age > 50 ? ValidationMode.Detailed : ValidationMode.Qucik;


            var validIdentity = _iIdentityValidator.IsValid(form.Applicant.IdNumber);
            if(!validIdentity)
            {
                return ApplicatonResult.TransferredToHR;
            }
            
            var similarTechStackCount = GetSimilarTechStackCount(form.TechStackList);
            if (similarTechStackCount < 25)
            {
                return ApplicatonResult.AutoReject;
            }


            if (similarTechStackCount > 75 &&
                form.YearsOfExperience >= autoAcceptedYearsOfExperience)
            {
                return ApplicatonResult.AutoAccept;
            }

            if (_iIdentityValidator.CountryDataProvider.CountyData.Country != "TURKEY")
            {
                return ApplicatonResult.TransferredToCTO;
            }



            return ApplicatonResult.AutoAccept;
        }

        private int GetSimilarTechStackCount(List<string> List)
        {
            int count = List
                .Where(x => TechStackList.Contains(x, StringComparer.OrdinalIgnoreCase))
                .Count();

            return (int)((double)(count / TechStackList.Count) * 100);
        }

        public enum ApplicatonResult
        {
            AutoReject,
            AutoAccept,
            TransferredToHR,
            TransferredToLead,
            TransferredToCTO,

        }


        // Default Dispose Pattern
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects)
                    _iIdentityValidator.Dispose();
                    _iIdentityValidator = null;

                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                disposedValue = true;
            }
        }

        ~ApplicationEvaluator()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
            
        }
    }
}
