﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MoneyFox.Shared.Interfaces;
using MoneyFox.Shared.Model;
using MoneyFox.Shared.StatisticDataProvider;
using Moq;

namespace MoneyFox.Shared.Tests.StatisticProvider
{
    [TestClass]
    public class CategorySummaryProviderTests
    {
        [TestMethod]
        [ExpectedException(typeof(NullReferenceException))]
        public void GetValues_NullDependency_NullReferenceException()
        {
            new CategorySummaryDataProvider(null).GetValues(DateTime.Today, DateTime.Today);
        }

        [TestMethod]
        public void GetValues_InitializedData_IgnoreTransfers()
        {
            //Setup

            var categoryRepoSetup = new Mock<IRepository<Category>>();
            categoryRepoSetup.SetupGet(x => x.Data).Returns(new ObservableCollection<Category>(new List<Category>
            {
                new Category {Id = 1, Name = "Ausgehen"}
            }));

            var categoryRepo = categoryRepoSetup.Object;

            var paymentRepoSetup = new Mock<IPaymentRepository>();
            paymentRepoSetup.SetupGet(x => x.Data).Returns(new ObservableCollection<Payment>(new List<Payment>
            {
                new Payment
                {
                    Id = 1,
                    Type = (int) PaymentType.Income,
                    Date = DateTime.Today,
                    Amount = 60,
                    Category = categoryRepo.Data.First(),
                    CategoryId = 1
                },
                new Payment
                {
                    Id = 2,
                    Type = (int) PaymentType.Expense,
                    Date = DateTime.Today,
                    Amount = 90,
                    Category = categoryRepo.Data.First(),
                    CategoryId = 1
                },
                new Payment
                {
                    Id = 3,
                    Type = (int) PaymentType.Transfer,
                    Date = DateTime.Today,
                    Amount = 40,
                    Category = categoryRepo.Data.First(),
                    CategoryId = 1
                }
            }));

            var unitOfWork = new Mock<IUnitOfWork>();
            unitOfWork.SetupGet(x => x.PaymentRepository).Returns(paymentRepoSetup.Object);
            unitOfWork.SetupGet(x => x.CategoryRepository).Returns(categoryRepo);

            //Excution
            var result =
                new CategorySummaryDataProvider(unitOfWork.Object).GetValues(DateTime.Today.AddDays(-3),
                    DateTime.Today.AddDays(3)).ToList();

            //Assertion
            result.Count.ShouldBe(1);
            result.First().Value.ShouldBe(-30);
        }

        [TestMethod]
        public void GetValues_InitializedData_CalculateIncome()
        {
            //Setup
            var categoryRepoSetup = new Mock<IRepository<Category>>();
            categoryRepoSetup.SetupGet(x => x.Data).Returns(new ObservableCollection<Category>(new List<Category>
            {
                new Category {Id = 1, Name = "Einkaufen"},
                new Category {Id = 2, Name = "Ausgehen"},
                new Category {Id = 3, Name = "Foo"}
            }));

            var categoryRepo = categoryRepoSetup.Object;

            var paymentRepoSetup = new Mock<IPaymentRepository>();
            paymentRepoSetup.SetupGet(x => x.Data).Returns(new ObservableCollection<Payment>(new List<Payment>
            {
                new Payment
                {
                    Id = 1,
                    Type = (int) PaymentType.Income,
                    Date = DateTime.Today,
                    Amount = 60,
                    Category = categoryRepo.Data[0],
                    CategoryId = 1
                },
                new Payment
                {
                    Id = 2,
                    Type = (int) PaymentType.Expense,
                    Date = DateTime.Today,
                    Amount = 90,
                    Category = categoryRepo.Data[0],
                    CategoryId = 1
                },
                new Payment
                {
                    Id = 3,
                    Type = (int) PaymentType.Expense,
                    Date = DateTime.Today,
                    Amount = 40,
                    Category = categoryRepo.Data[1],
                    CategoryId = 2
                },
                new Payment
                {
                    Id = 3,
                    Type = (int) PaymentType.Income,
                    Date = DateTime.Today,
                    Amount = 66,
                    Category = categoryRepo.Data[2],
                    CategoryId = 3
                }
            }));

            var unitOfWork = new Mock<IUnitOfWork>();
            unitOfWork.SetupGet(x => x.PaymentRepository).Returns(paymentRepoSetup.Object);
            unitOfWork.SetupGet(x => x.CategoryRepository).Returns(categoryRepo);

            //Excution
            var result =
                new CategorySummaryDataProvider(unitOfWork.Object).GetValues(DateTime.Today.AddDays(-3),
                    DateTime.Today.AddDays(3)).ToList();

            //Assertion
            result.Count.ShouldBe(3);
            result[0].Value.ShouldBe(-40);
            result[1].Value.ShouldBe(-30);
            result[2].Value.ShouldBe(66);
        }

        [TestMethod]
        public void GetValues_InitializedData_HandleDateCorrectly()
        {
            //Setup

            var categoryRepoSetup = new Mock<IRepository<Category>>();

            categoryRepoSetup.SetupGet(x => x.Data).Returns(new ObservableCollection<Category>(new List<Category>
            {
                new Category {Id = 1, Name = "Einkaufen"},
                new Category {Id = 2, Name = "Ausgehen"},
                new Category {Id = 3, Name = "Bier"}
            }));
            var categoryRepo = categoryRepoSetup.Object;

            var paymentRepoSetup = new Mock<IPaymentRepository>();
            paymentRepoSetup.SetupGet(x => x.Data).Returns(new ObservableCollection<Payment>(new List<Payment>
            {
                new Payment
                {
                    Id = 1,
                    Type = (int) PaymentType.Expense,
                    Date = DateTime.Today.AddDays(-5),
                    Amount = 60,
                    Category = categoryRepo.Data[0],
                    CategoryId = 1
                },
                new Payment
                {
                    Id = 2,
                    Type = (int) PaymentType.Expense,
                    Date = DateTime.Today,
                    Amount = 90,
                    Category = categoryRepo.Data[1],
                    CategoryId = 2
                },
                new Payment
                {
                    Id = 3,
                    Type = (int) PaymentType.Expense,
                    Date = DateTime.Today.AddDays(5),
                    Amount = 40,
                    Category = categoryRepo.Data[2],
                    CategoryId = 3
                }
            }));

            var unitOfWork = new Mock<IUnitOfWork>();
            unitOfWork.SetupGet(x => x.PaymentRepository).Returns(paymentRepoSetup.Object);
            unitOfWork.SetupGet(x => x.CategoryRepository).Returns(categoryRepo);

            //Excution
            var result =
                new CategorySummaryDataProvider(unitOfWork.Object).GetValues(DateTime.Today.AddDays(-3),
                    DateTime.Today.AddDays(3)).ToList();

            //Assertion
            result.Count.ShouldBe(1);
            result.First().Value.ShouldBe(-90);
        }
    }
}