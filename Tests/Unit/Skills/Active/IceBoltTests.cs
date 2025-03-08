using NUnit.Framework;
using Game.Skills;
using Game.Skills.Active;
using Game.Skills.Base;
using Game.Skills.Support;
using Game.Enemies;
using Moq;
using Godot;

namespace Game.Tests.Unit.Skills.Active
{
	[TestFixture]
	public class IceBoltTests
	{
		private IceBolt _iceBolt;
		private Mock<Node2D> _mockSource;
		private Mock<Monster> _mockMonster;

		[SetUp]
		public void Setup()
		{
			// 创建寒冰弹实例
			_iceBolt = new IceBolt();
			
			// 模拟源节点（玩家）
			_mockSource = new Mock<Node2D>();
			_mockSource.Setup(s => s.GlobalPosition).Returns(new Vector2(0, 0));
			_mockSource.Setup(s => s.GetGlobalMousePosition()).Returns(new Vector2(100, 0));
			
			// 模拟怪物
			_mockMonster = new Mock<Monster>();
			_mockMonster.Setup(m => m.GlobalPosition).Returns(new Vector2(50, 0));
			_mockMonster.Setup(m => m.Name).Returns("TestMonster");
		}

		[Test]
		public void Initialize_ShouldSetCorrectProperties()
		{
			// Act
			_iceBolt.Initialize();

			// Assert
			Assert.That(_iceBolt.Name, Is.EqualTo("IceBolt"));
			Assert.That(_iceBolt.Description, Is.EqualTo("Launch an ice bolt, dealing damage to the enemy and slowing them down."));
			Assert.That(_iceBolt.Cooldown, Is.EqualTo(2.0f));
			Assert.That(_iceBolt.ManaCost, Is.EqualTo(15.0f));
			Assert.That(_iceBolt.TriggerType, Is.EqualTo(SkillTriggerType.Active));
		}

		[Test]
		public void EnableMultiProjectiles_ShouldEnableMultishot()
		{
			// Act
			_iceBolt.EnableMultiProjectiles();
			
			// 由于_isMultishot是私有字段，我们需要通过反射来验证它的值
			var isMultishotField = _iceBolt.GetType()
				.GetField("_isMultishot", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
			bool isMultishot = (bool)isMultishotField.GetValue(_iceBolt);

			// Assert
			Assert.That(isMultishot, Is.True);
		}

		[Test]
		public void DisableMultiProjectiles_ShouldDisableMultishot()
		{
			// Arrange
			_iceBolt.EnableMultiProjectiles();
			
			// Act
			_iceBolt.DisableMultiProjectiles();
			
			// 通过反射获取私有字段值
			var isMultishotField = _iceBolt.GetType()
				.GetField("_isMultishot", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
			bool isMultishot = (bool)isMultishotField.GetValue(_iceBolt);

			// Assert
			Assert.That(isMultishot, Is.False);
		}

		[Test]
		public void AddSupport_WithLesserMultipleProjectiles_ShouldEnableMultishot()
		{
			// Arrange
			var support = new LesserMultipleProjectilesSupport();
			
			// Act
			_iceBolt.AddSupport(support);
			
			// 通过反射获取私有字段值
			var isMultishotField = _iceBolt.GetType()
				.GetField("_isMultishot", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
			bool isMultishot = (bool)isMultishotField.GetValue(_iceBolt);

			// Assert
			Assert.That(isMultishot, Is.True);
		}

		[Test]
		public void Trigger_ShouldStartCooldown()
		{
			// Arrange
			_iceBolt.Initialize();
			
			// Act
			_iceBolt.Trigger(_mockSource.Object);
			
			// Assert
			Assert.That(_iceBolt.CurrentCooldown, Is.GreaterThan(0));
			Assert.That(_iceBolt.CurrentCooldown, Is.EqualTo(_iceBolt.Cooldown));
		}

		[Test]
		public void Update_ShouldReduceCooldown()
		{
			// Arrange
			_iceBolt.Initialize();
			_iceBolt.Trigger(_mockSource.Object);
			float initialCooldown = _iceBolt.CurrentCooldown;
			float deltaTime = 0.5f;
			
			// Act
			_iceBolt.Update(deltaTime);
			
			// Assert
			Assert.That(_iceBolt.CurrentCooldown, Is.EqualTo(initialCooldown - deltaTime));
		}

		[Test]
		public void CanTrigger_WithZeroCooldown_ShouldReturnTrue()
		{
			// Arrange
			_iceBolt.Initialize();
			
			// Act & Assert
			Assert.That(_iceBolt.CanTrigger(), Is.True);
		}

		[Test]
		public void CanTrigger_DuringCooldown_ShouldReturnFalse()
		{
			// Arrange
			_iceBolt.Initialize();
			_iceBolt.Trigger(_mockSource.Object);
			
			// Act & Assert
			Assert.That(_iceBolt.CanTrigger(), Is.False);
		}

		[Test]
		public void GetAimDirection_ShouldReturnNormalizedDirection()
		{
			// Arrange
			var getAimDirectionMethod = _iceBolt.GetType()
				.GetMethod("GetAimDirection", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
			
			// Act
			Vector2 direction = (Vector2)getAimDirectionMethod.Invoke(_iceBolt, new object[] { _mockSource.Object });
			
			// Assert
			Assert.That(direction.Length(), Is.EqualTo(1).Within(0.001f));
			Assert.That(direction.X, Is.EqualTo(1).Within(0.001f));
			Assert.That(direction.Y, Is.EqualTo(0).Within(0.001f));
		}

		[Test]
		public void IceBolt_DamageConstant_ShouldMatchExpected()
		{
			// 通过反射获取私有常量值
			var damageField = _iceBolt.GetType()
				.GetField("DAMAGE", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
			float damage = (float)damageField.GetValue(null);

			// Assert
			Assert.That(damage, Is.EqualTo(20f));
		}

		[Test]
		public void IceBolt_SlowFactorConstant_ShouldMatchExpected()
		{
			// 通过反射获取私有常量值
			var slowFactorField = _iceBolt.GetType()
				.GetField("SLOW_FACTOR", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
			float slowFactor = (float)slowFactorField.GetValue(null);

			// Assert
			Assert.That(slowFactor, Is.EqualTo(0.5f));
		}

		[Test]
		public void IceBolt_SlowDurationConstant_ShouldMatchExpected()
		{
			// 通过反射获取私有常量值
			var slowDurationField = _iceBolt.GetType()
				.GetField("SLOW_DURATION", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
			float slowDuration = (float)slowDurationField.GetValue(null);

			// Assert
			Assert.That(slowDuration, Is.EqualTo(3.0f));
		}
	}
} 
