using API_CF_Demo1.Data;
using API_CF_Demo1.Models;
using API_CF_Demo1.Repositories;
using Microsoft.EntityFrameworkCore;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace API_CF_Service_Test_Project
{
    internal class DeparmentServiceTest
    {
        private Mock<MyDbContext> _mockContext;
        private DepartmentService _departmentService;
        private List<Department> _departments;

        [SetUp]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<MyDbContext>().                
                UseInMemoryDatabase(databaseName: "TestDatabase").Options;
            _mockContext = new Mock<MyDbContext>(options);
            _departments = new List<Department>
        {
            new Department { Id = 1, Name = "HR", DepartmentHead = "John Doe" },
            new Department { Id = 2, Name = "Finance", DepartmentHead = "Jane Smith" }
        };

            var mockDepartmentSet = new Mock<DbSet<Department>>();          
            var queryableDepartments = _departments.AsQueryable();

            mockDepartmentSet.As<IQueryable<Department>>().Setup(m => m.Provider).Returns(queryableDepartments.Provider);
            mockDepartmentSet.As<IQueryable<Department>>().Setup(m => m.Expression).Returns(queryableDepartments.Expression);
            mockDepartmentSet.As<IQueryable<Department>>().Setup(m => m.ElementType).Returns(queryableDepartments.ElementType);
            mockDepartmentSet.As<IQueryable<Department>>().Setup(m => m.GetEnumerator()).
                Returns(queryableDepartments.GetEnumerator());


            //mockDepartmentSet.Setup(m => m.Find(It.IsAny<object[]>())).Returns<object[]>(ids => 
            //_departments.SingleOrDefault(d => d.Id == (int)ids[0]));
            _mockContext.Setup(c => c.Departments).Returns(mockDepartmentSet.Object);
            _departmentService = new DepartmentService(_mockContext.Object);
        }

        [Test]
        public void GetAllDepartments_Should_AllDepartments()
        {
            var result = _departmentService.GetAllDepartments();
            Assert.NotNull(result);
            Assert.That(result.Count, Is.EqualTo(2));
        }
        [Test]
        public void SearchByName_Should_ShouldReturnMatchingDepartments()
        {
            var result = _departmentService.SearchByName("HR");
            Assert.NotNull(result);
            Assert.That(result.Count, Is.EqualTo(1));
            Assert.That(result.First().Name, Is.EqualTo("HR"));
        }
        [Test]
        public void AddNewDepartment_shouldReturnAddDepartmentId()
        {
            var newDepartment = new Department() { Id = 3, Name = "Finance", DepartmentHead = "Nithin" };
            var result = _departmentService.AddNewDepartment(newDepartment);
            Assert.That(result, Is.EqualTo(3));
            _mockContext.Verify( m=> m.Departments.Add(It.IsAny<Department>()),Times.Once);
            _mockContext.Verify(m => m.SaveChanges(),Times.Once);
        }

        [Test]
        public void DeleteDepartment_shouldRemoveDepartment()
        {
           
            var result = _departmentService.DeleteDepartment(1);
            Assert.That(result, Is.EqualTo("the given Department id 1 Removed"));
            _mockContext.Verify(m => m.Departments.Remove(It.IsAny<Department>()), Times.Once);
            _mockContext.Verify(m => m.SaveChanges(), Times.Once);
        }
        [Test]
        public void DeleteDepartment_ShouldReturnError_WhenDepartmentIdNotFound()
        {
            var result = _departmentService.DeleteDepartment(3);
            Assert.That(result, Is.EqualTo("Something went wrong with deletion"));
            _mockContext.Verify(m => m.Departments.Remove(It.IsAny<Department>()), Times.Never);
            _mockContext.Verify(m => m.SaveChanges(), Times.Never);
        }
        [Test]
        public void GetDepartmentById_ShouldReturnCorrectDepartment()
        {
            var result = _departmentService.GetDepartmentById(1);
           Assert.NotNull(result);
            Assert.That(result.Name,Is.EqualTo("HR"));
        }
        [Test]
        public void GetDepartmentById_ShouldReturnNull_WhenDepartmentIdNotFound()
        {
            var result = _departmentService.GetDepartmentById(3);
            Assert.IsNull(result);
        }
        [Test]
        public void UpdateDepartment_ShouldUpdateDepartment_WhenDepartmentExists()
        {
            var updateDepartment = new Department { Id = 1, Name = "Human Resource",
                DepartmentHead = "Sam" };

            var result = _departmentService.UpdateDepartment(updateDepartment);
            Assert.That(result, Is.EqualTo("Record Updated successfully"));
            _mockContext.Verify(m => m.SaveChanges(), Times.Once);
         
        }
        [Test]
        public void UpdateDepartment_ShouldReturnError_WhenDepartmentDoesNotExists()
        {
            var updateDepartment = new Department { Id = 3, Name = "No Department", DepartmentHead = "Tim" };
            var result = _departmentService.UpdateDepartment(updateDepartment);
            Assert.That(result, Is.EqualTo("something went wrong while update"));
            _mockContext.Verify(m => m.SaveChanges(), Times.Never);

        }
    }
}

