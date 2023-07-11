using NUnit.Framework;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using JobApplicationLibrary.Models;
using static JobApplicationLibrary.ApplicationEvaluator;
using Assert = NUnit.Framework.Assert;
using Moq;
using JobApplicationLibrary.Services;
using FluentAssertions;

namespace JobApplicationLibrary.Tests
{
    
    public class ApplicationEvaluatorTests
    {
        private Mock<IIdentityValidator> InitialiseTestMock()
        {
            var mock = new Mock<IIdentityValidator>();
            mock.DefaultValue = DefaultValue.Mock;
            mock.Setup(i => i.CountryDataProvider.CountyData.Country).Returns("TURKEY");
            mock.Setup(i => i.IsValid(It.IsAny<string>())).Returns(true);

            return mock;
            
            
        }

        private ApplicationEvaluator InitialiseTestEvaluator(Mock<IIdentityValidator> mock)
        {
            ApplicationEvaluator evaluator = new ApplicationEvaluator(mock.Object);
            return evaluator;
        }

        private JobApplication InitialiseTestJobApplictaion()
        {
            JobApplication form = new JobApplication();
            form.Applicant = new Applicant();
            form.Applicant.Age = 18;
            form.Applicant.IdNumber = "12345678910";
            form.TechStackList = new List<string>() { "C#", "RabbitMQ", "Docker", "Microservice", "VisualStudio" };
            return form;

        }


        // Yaş 18'den küçükse, AutoReject mi?
        [Test]
        public void Application_WithUnderAge_TransferredToAutoRejected()
        {
            // Arrange
            var mock = InitialiseTestMock();
            using var evaluator = InitialiseTestEvaluator(mock);

            using var form = InitialiseTestJobApplictaion();
            form.Applicant.Age = 17; // Test Case

            // Act
            var result = evaluator.Evaluate(form);

            // Assert
            // Assert.AreEqual(ApplicatonResult.AutoReject, result); Aşağıdaki ile aynı
            result.Should().Be(ApplicatonResult.AutoReject);
        }

        // TechStackList'te yüzde 25'ten az ortak teknoloji varsa, AutoReject mi?
        [Test]
        public void Application_WithNoTechStack_TransferredToAutoRejected()
        {
            // Arrange
            var mock = InitialiseTestMock();
            using var evaluator = InitialiseTestEvaluator(mock);
            using var form = InitialiseTestJobApplictaion();
            form.TechStackList = new List<string>() { "C#" }; // Test Case

            // Act
            var result = evaluator.Evaluate(form);

            // Assert
            //Assert.AreEqual(ApplicatonResult.AutoReject, result); Aşağıdaki ile aynı
            result.Should().Be(ApplicatonResult.AutoReject);
        }

        // TechStackList'te yüzde 75'ten fazla ortak teknoloji varsa, AutoAccept mi?
        [Test]
        public void Application_WithTechStackOver75P_TranserToAutoAccepted()
        {
            // Arrange
            var mock = InitialiseTestMock(); //Test case default olarak 75 üstü yüzde veriyor
            using var evaluator = InitialiseTestEvaluator(mock);
            using var form = InitialiseTestJobApplictaion();
            form.YearsOfExperience = 11; 

            // Act
            var result = evaluator.Evaluate(form);

            // Assert
            //Assert.AreEqual(ApplicatonResult.AutoAccept, result); Aşağıdaki ile aynı 
            result.Should().Be(ApplicatonResult.AutoAccept);

        }

        // IdentityNumber geçerli değilse, HR'a gönderiyor mu?
        [Test]
        public void Application_WithInvalidIdentityNumber_TransferredToHR()
        {
            // Arrange
            var mock = InitialiseTestMock();
            mock.Setup(i => i.IsValid(It.IsAny<string>())).Returns(false); // Test Case
            using var evaluator = InitialiseTestEvaluator(mock);
            using var form = InitialiseTestJobApplictaion();

            // Act
            var result = evaluator.Evaluate(form);

            // Assert
            //Assert.IsTrue(result == ApplicatonResult.TransferredToHR); Aşağıdaki ile aynı
            result.Should().Be(ApplicatonResult.TransferredToHR);
        }


        // Çalışma yeri istanbul olmazsa, CTO'ya gönderiyor mu?
        [Test]
        public void Application_WithOfficeLocation_TransferredToCTO()
        {
            // Arrange
            var mock = InitialiseTestMock();
            mock.Setup(i => i.CountryDataProvider.CountyData.Country).Returns("SPAIN"); // Test Case

            using var evaluator = InitialiseTestEvaluator(mock);
            using var form = InitialiseTestJobApplictaion();

            // Act
            var result = evaluator.Evaluate(form);

            // Assert
            //Assert.AreEqual(ApplicatonResult.TransferredToCTO, result); Aşağıdaki ile aynı
            result.Should().Be(ApplicatonResult.TransferredToCTO);
        }


        // Yaş 50'den fazla olunca, ValidationMode'un Detailed olup olmadığını kontrol ediyor mu?
        [Test]
        public void Application_WithAgeOver50_ValidationModeToDetailed()
        {
            // Arrange
            var mock = InitialiseTestMock();
            mock.SetupProperty(i => i.ValidationMode); /*mock değerini kontrol etmek için setup ediyoruz*/

            using var evaluator = InitialiseTestEvaluator(mock);

            using var form = InitialiseTestJobApplictaion();
            form.Applicant.Age = 51; //Test Case

            // Act
            var result = evaluator.Evaluate(form);

            // Assert
            //Assert.AreEqual(ValidationMode.Detailed, mock.Object.ValidationMode); Aşağıdaki ile aynı
            mock.Object.ValidationMode.Should().Be(ValidationMode.Detailed);
        }

        // Null değer gönderildiğinde ArgumentNullException fırlatıyor mu?
        [Test]
        public void Application_WithNullApplicant_ThrowsArgumentNullException()
        {
            // Arrange
            var mock = InitialiseTestMock();  
            using var evaluator = InitialiseTestEvaluator(mock);
            var form = InitialiseTestJobApplictaion();

                form.Applicant = null; // Test Case

                // Act
                Action actionResult = () => evaluator.Evaluate(form);

                // Assert
                actionResult.Should().Throw<ArgumentNullException>();

        }


        // IsValid yalnızca 1 kere çağrılmış mı?
        [Test]
        public void Application_WithDefaultValue_IsValidCalled()
        {
            // Arrange
            var mock = InitialiseTestMock();

            using var evaluator = InitialiseTestEvaluator(mock);
            using var form = InitialiseTestJobApplictaion();
            
            // Act
            var result = evaluator.Evaluate(form);

            // Assert
            mock.Verify(i => i.IsValid(It.IsAny<string>()), Times.Exactly(1));

        }


        // Yaşı 18'den küçük olduğu için IsValid çağrılmamalı
        [Test]
        public void Application_WithYoungAge_IsValidNeverCalled()
        {
            // Arrange
            var mock = InitialiseTestMock();
            using var evaluator = InitialiseTestEvaluator(mock);
            using var form = InitialiseTestJobApplictaion();
            form.Applicant.Age = 17; // Test Case

            // Act
            var result = evaluator.Evaluate(form);

            // Assert
            mock.Verify(i => i.IsValid(It.IsAny<string>()), Times.Exactly(0));
        }
    }
}