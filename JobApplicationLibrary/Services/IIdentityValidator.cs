using JobApplicationLibrary.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JobApplicationLibrary.Services
{
    public interface IIdentityValidator
    {
        bool IsValid(string idNumber);
        
        ICountryDataProvider CountryDataProvider { get; }

        public ValidationMode ValidationMode { get; set; }
    }


    public interface ICountyData
    {
        string Country { get; }
    }

    public interface ICountryDataProvider
    {
        ICountyData CountyData { get; }
    }

    public enum ValidationMode
    {
        None,
        Detailed,
        Qucik
    }
}
