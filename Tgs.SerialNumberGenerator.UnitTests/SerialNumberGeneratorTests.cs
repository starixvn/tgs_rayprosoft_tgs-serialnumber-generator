using NSubstitute;

namespace Tgs.SerialNumberGenerator.UnitTests;

public class SerialNumberGeneratorTests
{
	[Fact]
	public async Task 產生流水號()
	{
		// Arrange
		var sut = new SerialNumberGenerator(stackalloc byte[4], new DefaultSystemClock());
		_ = sut.StartListenAsync();

		// Act
		var actual = await sut.GenerateAsync();

		// Assert
		Assert.NotEqual(Guid.Empty, actual);
	}

	[Fact]
	public async Task 產生流水號_後面時間的流水號會比前面的大()
	{
		// Arrange
		var fakeSystemClock = Substitute.For<ISystemClock>();
		var sut = new SerialNumberGenerator(stackalloc byte[4], fakeSystemClock);
		_ = sut.StartListenAsync();

		_ = fakeSystemClock.UtcNow
			.Returns(
				DateTimeOffset.FromUnixTimeMilliseconds(0x0000_0100_0000_1001),
				DateTimeOffset.FromUnixTimeMilliseconds(0x0000_0101_0000_0001));

		// Act
		var actual1 = await sut.GenerateAsync();
		var actual2 = await sut.GenerateAsync();

		// Assert
		Assert.Equal(1, actual2.CompareTo(actual1));
	}

	[Fact]
	public async Task 產生流水號_時間低位元比較()
	{
		// Arrange
		var fakeSystemClock = Substitute.For<ISystemClock>();
		var sut = new SerialNumberGenerator(stackalloc byte[4], fakeSystemClock);
		_ = sut.StartListenAsync();

		_ = fakeSystemClock.UtcNow
			.Returns(
				DateTimeOffset.FromUnixTimeMilliseconds(0x0000_01000000_0002),
				DateTimeOffset.FromUnixTimeMilliseconds(0x0000_01000000_0101));

		// Act
		var actual1 = await sut.GenerateAsync();
		var actual2 = await sut.GenerateAsync();

		// Assert
		Assert.Equal(1, actual2.CompareTo(actual1));
	}

	[Fact]
	public async Task 產生流水號_時間高位元比較()
	{
		// Arrange
		var fakeSystemClock = Substitute.For<ISystemClock>();
		var sut = new SerialNumberGenerator(stackalloc byte[4], fakeSystemClock);
		_ = sut.StartListenAsync();

		_ = fakeSystemClock.UtcNow
			.Returns(
				DateTimeOffset.FromUnixTimeMilliseconds(0x0000_01F0_0000_FFF2),
				DateTimeOffset.FromUnixTimeMilliseconds(0x0000_0200_0000_0101));

		// Act
		var actual1 = await sut.GenerateAsync();
		var actual2 = await sut.GenerateAsync();

		// Assert
		Assert.Equal(1, actual2.CompareTo(actual1));
	}
}
