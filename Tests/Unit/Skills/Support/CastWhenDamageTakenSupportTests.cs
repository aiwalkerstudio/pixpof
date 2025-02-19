using NUnit.Framework;
using Game.Skills.Support;
using Game.Skills.Base;
using Game.Skills.Active;
using Game.Skills;
using Moq;
using Godot;

namespace Game.Tests.Unit.Skills.Support
{
	[TestFixture]
	public class CastWhenDamageTakenSupportTests
	{
		private CastWhenDamageTakenSupport _support;
		private Mock<Skill> _mockActiveSkill;
		private Mock<Node> _mockSource;

		[SetUp]
		public void Setup()
		{
			_support = new CastWhenDamageTakenSupport();
			_mockActiveSkill = new Mock<Skill>();
			_mockSource = new Mock<Node>();
			
			// 设置模拟技能
			_mockActiveSkill.Setup(s => s.TriggerType).Returns(SkillTriggerType.Active);
			_mockActiveSkill.Setup(s => s.HasReservation).Returns(false);
			_mockActiveSkill.Setup(s => s.IsChanneling).Returns(false);
			_mockActiveSkill.Setup(s => s.CanTrigger()).Returns(true);
			_mockActiveSkill.Setup(s => s.Name).Returns("测试技能");
		}

		[Test]
		public void Initialize_ShouldSetCorrectInitialState()
		{
			// Act
			_support.Initialize();

			// Assert
			Assert.That(_support.Name, Is.EqualTo("受伤时施放"));
			Assert.That(_support.Cooldown, Is.EqualTo(0.25f));
		}

		[Test]
		public void OnDamageTaken_BelowThreshold_ShouldNotTrigger()
		{
			// Arrange
			_support.Initialize();
			_support.LinkSkill(_mockActiveSkill.Object);

			// Act
			_support.OnDamageTaken(5f);

			// Assert
			_mockActiveSkill.Verify(s => s.Trigger(It.IsAny<Node>()), Times.Never);
		}

		[Test]
		public void OnDamageTaken_AboveThreshold_ShouldTrigger()
		{
			// Arrange
			_support.Initialize();
			_support.LinkSkill(_mockActiveSkill.Object);

			// Act
			_support.OnDamageTaken(15f);

			// Assert
			_mockActiveSkill.Verify(s => s.Trigger(It.IsAny<Node>()), Times.Once);
		}

		[Test]
		public void OnDamageTaken_DuringCooldown_ShouldNotTrigger()
		{
			// Arrange
			_support.Initialize();
			_support.LinkSkill(_mockActiveSkill.Object);
			
			// 先触发一次
			_support.OnDamageTaken(15f);
			
			// 冷却期间再次触发
			_support.OnDamageTaken(15f);

			// Assert
			_mockActiveSkill.Verify(s => s.Trigger(It.IsAny<Node>()), Times.Once);
		}

		[Test]
		public void CanLinkSkill_WithValidActiveSkill_ShouldSucceed()
		{
			// Arrange
			var activeSkill = new Fireball();

			// Act
			_support.LinkSkill(activeSkill);

			// Assert
			// 验证技能是否被成功链接
			var linkedSkills = _support.GetType()
				.GetField("LinkedSkills", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
				?.GetValue(_support) as System.Collections.Generic.List<Skill>;
			
			Assert.That(linkedSkills, Is.Not.Null);
			Assert.That(linkedSkills.Contains(activeSkill), Is.True);
		}

		[Test]
		public void CanLinkSkill_WithReservationSkill_ShouldNotLink()
		{
			// Arrange
			_mockActiveSkill.Setup(s => s.HasReservation).Returns(true);

			// Act
			_support.LinkSkill(_mockActiveSkill.Object);

			// Assert
			var linkedSkills = _support.GetType()
				.GetField("LinkedSkills", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
				?.GetValue(_support) as System.Collections.Generic.List<Skill>;
			
			Assert.That(linkedSkills, Is.Not.Null);
			Assert.That(linkedSkills.Contains(_mockActiveSkill.Object), Is.False);
		}

		[Test]
		public void OnDamageTaken_MultipleHits_ShouldAccumulateDamage()
		{
			// Arrange
			_support.Initialize();
			_support.LinkSkill(_mockActiveSkill.Object);

			// Act
			_support.OnDamageTaken(4f);
			_support.OnDamageTaken(3f);
			_support.OnDamageTaken(4f);

			// Assert
			_mockActiveSkill.Verify(s => s.Trigger(It.IsAny<Node>()), Times.Once);
		}

		[Test]
		public void OnDamageTaken_LinkedSkillOnCooldown_ShouldNotTrigger()
		{
			// Arrange
			_support.Initialize();
			_mockActiveSkill.Setup(s => s.CanTrigger()).Returns(false);
			_support.LinkSkill(_mockActiveSkill.Object);

			// Act
			_support.OnDamageTaken(15f);

			// Assert
			_mockActiveSkill.Verify(s => s.Trigger(It.IsAny<Node>()), Times.Never);
		}
	}
} 
