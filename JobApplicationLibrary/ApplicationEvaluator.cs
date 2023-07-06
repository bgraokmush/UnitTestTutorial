using JobApplicationLibrary.Models;
using JobApplicationLibrary.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JobApplicationLibrary
{
    public class ApplicationEvaluator   
    {
        private const int minAge = 18;
        private const int autoAcceptedYearsOfExperience = 10;
        public object mock;
        private List<string> TechStackList = new() { "C#", "RabbitMQ", "Docker", "Microservice", "VisualStudio"};
        private IIdentityValidator _iIdentityValidator;

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
    }
}
