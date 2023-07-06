using JobApplicationLibrary.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JobApplicationLibrary.Services
{
    public interface IIdentityValidator : IDisposable
    {
        bool IsValid(string idNumber);
        
        ICountryDataProvider CountryDataProvider { get; }

        public ValidationMode ValidationMode { get; set; }
    }


    public interface ICountyData : IDisposable
    {
        string Country { get; }
    }

    public interface ICountryDataProvider : IDisposable
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
