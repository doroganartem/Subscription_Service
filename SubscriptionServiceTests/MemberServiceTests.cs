using Moq;
using Subscription_Service.Services.Interfaces;
using Subscription_Service.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Subscription_Service.Models;

namespace SubscriptionServiceTests
{
    public class MemberServiceTests
    {
        private readonly Mock<IMemberRepository> _repo = new();
        private readonly Mock<IPaymentService> _payment = new();
        private readonly Mock<INotificationService> _notify = new();
        private readonly MemberService _service;
        public MemberServiceTests()
        {
            _service = new MemberService(_repo.Object);
        }

        /// <summary>
        /// Перевіряє, що GetMember повертає об'єкт Member, якщо запис існує в репозиторії.
        /// </summary>
        [Fact]
        public void GetMember_WhenMemberExists_ReturnsMemberObject()
        {
            //Arrange 
            int testId = 1;
            var expectedMember = new Member { Id = testId, IsActive = true };

            _repo.Setup(r => r.GetById(testId)).Returns(expectedMember);

            //Act 
            var result = _service.GetMember(testId);

            //Assert 
            Assert.NotNull(result); 
            Assert.Equal(testId, result.Id);
            _repo.Verify(r => r.GetById(testId), Times.Once); 
        }

        /// <summary>
        /// Перевіряє роботу IsActive для різних ID
        /// </summary>
        [Theory]
        [InlineData(1, true)]
        [InlineData(2, false)]
        public void IsActive_ReturnsExpectedResult_BasedOnMemberStatus(int memberId, bool isActiveStatus)
        {
            // Arrange
            var member = new Member { Id = memberId, IsActive = isActiveStatus };
            _repo.Setup(r => r.GetById(memberId)).Returns(member);

            // Act
            var result = _service.IsActive(memberId);

            // Assert
            if (isActiveStatus)
            {
                Assert.True(result); 
            }
            else
            {
                Assert.False(result); 
            }
        }

        /// <summary>
        /// Перевіряє роботу GetMember, коли об'єкт відстуній
        /// </summary>
        [Fact]
        public void GetMember_WhenMemberNotExist_ReturnNull()
        {
            // Arrange
            _repo.Setup(r => r.GetById(99)).Returns((Member?)null);

            // Act
            var result = _service.GetMember(99);

            Assert.Null(result);
        }
    }
}
