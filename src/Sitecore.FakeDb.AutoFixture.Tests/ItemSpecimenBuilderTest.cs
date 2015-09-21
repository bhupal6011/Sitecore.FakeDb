namespace Sitecore.FakeDb.AutoFixture.Tests
{
  using FluentAssertions;
  using Ploeh.AutoFixture;
  using Ploeh.AutoFixture.Kernel;
  using Sitecore.Data.Items;
  using Xunit;

  public class ItemSpecimenBuilderTest
  {
    [Fact]
    public void SutIsSpecimenBuilder()
    {
      var sut = new ItemSpecimenBuilder();
      sut.Should().BeAssignableTo<ISpecimenBuilder>();
    }

    [Fact]
    public void CreateReturnsNoSpecimentIfNoItemRequested()
    {
      var sut = new ItemSpecimenBuilder();
      sut.Create(new object(), null).Should().BeOfType<NoSpecimen>();
    }

    [Fact]
    public void CreateReturnsItemInstance()
    {
      var fixture = new Fixture();
      fixture.Customizations.Add(new ItemSpecimenBuilder());

      fixture.Create<Item>().Should().NotBeNull();
    }
  }
}