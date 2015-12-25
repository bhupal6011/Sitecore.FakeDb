﻿namespace Sitecore.FakeDb.Tests
{
  using System;
  using System.Collections.Generic;
  using FluentAssertions;
  using Ploeh.AutoFixture.Xunit2;
  using Sitecore.Data;
  using Sitecore.Globalization;
  using Xunit;

  public class DbFieldTest
  {
    private readonly DbField field;

    public DbFieldTest()
    {
      this.field = new DbField("Title");
    }

    [Fact]
    public void ShouldSetName()
    {
      // act & assert
      this.field.Name.Should().Be("Title");
    }

    [Fact]
    public void ShouldSetType()
    {
      // arrange
      this.field.Type = "Single-Line Text";

      // act & assert
      this.field.Type.Should().Be("Single-Line Text");
    }

    [Fact]
    public void ShouldSetSource()
    {
      // arrange
      this.field.Source = "/sitecore/content";

      // act & assert
      this.field.Source.Should().Be("/sitecore/content");
    }

    [Fact]
    public void ShouldInstantiateVersionsAsSortedDictionary()
    {
      // act
      this.field.Add("en", "value");

      // assert
      this.field.Values["en"].Should().BeOfType<SortedDictionary<int, string>>();
    }

    [Fact]
    public void ShouldAddAndGetLocalizedValues()
    {
      // act
      this.field.Add("en", "en_value");
      this.field.Add("da", "da_value");

      // assert
      this.field.GetValue("en", 1).Should().Be("en_value");
      this.field.GetValue("da", 1).Should().Be("da_value");
    }

    [Fact]
    public void ShouldAddAndGetVersionedValues()
    {
      // act
      this.field.Add("en", 1, "en_value1");
      this.field.Add("en", 2, "en_value2");
      this.field.Add("da", 1, "da_value1");
      this.field.Add("da", 2, "da_value2");

      // assert
      this.field.GetValue("en", 1).Should().Be("en_value1");
      this.field.GetValue("en", 2).Should().Be("en_value2");
      this.field.GetValue("da", 1).Should().Be("da_value1");
      this.field.GetValue("da", 2).Should().Be("da_value2");
    }

    [Fact]
    public void ShouldGetValueInCurrentLanguage()
    {
      // arrange
      this.field.Add("en", "en_value");
      this.field.Add("da", "da_value");

      var language = Language.Parse("da");

      // act
      using (new LanguageSwitcher(language))
      {
        this.field.Value.Should().Be("da_value");
      }
    }

    [Fact]
    public void ShouldGetEmptyStringIfNoVersionFound()
    {
      // arrange
      this.field.Add("en", 1, "value");

      // assert
      this.field.GetValue("en", 100).Should().BeEmpty();
    }

    [Fact]
    public void ShouldSetAndGetValueInCurrentLanguage()
    {
      // act
      this.field.Value = "Hi there!";

      // assert
      this.field.Value.Should().Be("Hi there!");
    }

    [Fact]
    public void ShouldReturnEmptyStringIfNoValueFoundInCurrentLanguage()
    {
      // arrange
      this.field.Add("en", "en_value");

      // act & assert
      using (new LanguageSwitcher(Language.Parse("da")))
      {
        this.field.Value.Should().BeEmpty();
      }
    }

    [Fact]
    public void ShouldAddFewVersionsWithoutSpecifyingVersionNumber()
    {
      // act
      this.field.Add("en", "v1");
      this.field.Add("en", "v2");

      // assert
      this.field.Values["en"][1].Should().Be("v1");
      this.field.Values["en"][2].Should().Be("v2");
    }

    [Fact]
    public void ShouldAddVersionsImplicitly()
    {
      // act
      this.field.Add("en", 3, "Hello!");

      // assert
      this.field.Values["en"][1].Should().BeEmpty();
      this.field.Values["en"][2].Should().BeEmpty();
      this.field.Values["en"][3].Should().Be("Hello!");
    }

    [Fact]
    public void ShouldNotOverrideExistingVersionWhenAddingVersionsImplicitly()
    {
      // act
      this.field.Add("en", 1, "Hello v1!");
      this.field.Add("en", 3, "Hello v3!");

      // assert
      this.field.Values["en"][1].Should().Be("Hello v1!");
      this.field.Values["en"][2].Should().BeEmpty();
      this.field.Values["en"][3].Should().Be("Hello v3!");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void ShouldThrowExceptionIfVersionIsNotPositive(int version)
    {
      // act
      Action action = () => this.field.Add("en", version, "value");

      // assert
      action.ShouldThrow<ArgumentOutOfRangeException>().WithMessage("Version cannot be zero or negative.*");
    }

    [Fact]
    public void ShouldThrowExceptionIfVersionExists()
    {
      // arrange
      this.field.Add("en", 1, "value");

      // act
      Action action = () => this.field.Add("en", 1, "value");

      // assert
      action.ShouldThrow<ArgumentException>().WithMessage("An item with the same version has already been added.");
    }

    [Fact]
    public void ShouldSetAndGetSharedFieldValue()
    {
      // arrange
      this.field.Shared = true;

      // act
      this.field.Value = "shared value";

      // assert
      this.field.Value.Should().Be("shared value");
    }

    [Fact]
    public void ShouldGetEmptySharedFieldValueByDefault()
    {
      // arrange
      this.field.Shared = true;

      // act & assert
      this.field.Value.Should().BeEmpty();
    }

    [Fact]
    public void ShouldIgnoreLocalizedVersionsIfShared()
    {
      // arrange
      this.field.Shared = true;

      // act
      this.field.Add("en", "shared value");
      this.field.Add("da", "new shared value");

      // assert
      this.field.Value.Should().Be("new shared value");
    }

    [Theory]
    [InlineData("{12C33F3F-86C5-43A5-AEB4-5598CEC45116}", "__Base template")]
    [InlineData("{25BED78C-4957-4165-998A-CA1B52F67497}", "__Created")]
    [InlineData("{5DD74568-4D4B-44C1-B513-0AF5F4CDA34F}", "__Created by")]
    [InlineData("{39C4902E-9960-4469-AEEF-E878E9C8218F}", "__Hidden")]
    [InlineData("{001DD393-96C5-490B-924A-B0F25CD9EFD8}", "__Lock")]
    [InlineData("{9C6106EA-7A5A-48E2-8CAD-F0F693B1E2D4}", "__Read Only")]
    [InlineData("{F1A1FE9E-A60C-4DDB-A3A0-BB5B29FE732E}", "__Renderings")]
    [InlineData("{8CDC337E-A112-42FB-BBB4-4143751E123F}", "__Revision")]
    [InlineData("{DEC8D2D5-E3CF-48B6-A653-8E69E2716641}", "__Security")]
    [InlineData("{F7D48A55-2158-4F02-9356-756654404F73}", "__Standard values")]
    [InlineData("{D9CF14B1-FA16-4BA6-9288-E8A174D4D522}", "__Updated")]
    [InlineData("{BADD9CF9-53E0-4D0C-BCC0-2D784C282F6A}", "__Updated by")]
    public void ShouldMapDefaultFieldNameById(string fieldId, string expectedName)
    {
      // arrange
      var id = new ID(fieldId);

      // act
      var dbfield = new DbField(id);

      // assert
      dbfield.Name.Should().Be(expectedName);
    }

    [Theory]
    [InlineData("__Base template", "{12C33F3F-86C5-43A5-AEB4-5598CEC45116}")]
    [InlineData("__Created", "{25BED78C-4957-4165-998A-CA1B52F67497}")]
    [InlineData("__Created by", "{5DD74568-4D4B-44C1-B513-0AF5F4CDA34F}")]
    [InlineData("__Hidden", "{39C4902E-9960-4469-AEEF-E878E9C8218F}")]
    [InlineData("__Lock", "{001DD393-96C5-490B-924A-B0F25CD9EFD8}")]
    [InlineData("__Read Only", "{9C6106EA-7A5A-48E2-8CAD-F0F693B1E2D4}")]
    [InlineData("__Renderings", "{F1A1FE9E-A60C-4DDB-A3A0-BB5B29FE732E}")]
    [InlineData("__Revision", "{8CDC337E-A112-42FB-BBB4-4143751E123F}")]
    [InlineData("__Security", "{DEC8D2D5-E3CF-48B6-A653-8E69E2716641}")]
    [InlineData("__Standard values", "{F7D48A55-2158-4F02-9356-756654404F73}")]
    [InlineData("__Updated", "{D9CF14B1-FA16-4BA6-9288-E8A174D4D522}")]
    [InlineData("__Updated by", "{BADD9CF9-53E0-4D0C-BCC0-2D784C282F6A}")]
    public void ShouldMapDefaultFieldIdByName(string fieldName, string expectedId)
    {
      // act
      var dbfield = new DbField(fieldName);

      // assert
      dbfield.ID.ToString().Should().Be(expectedId);
    }

    [Fact]
    public void ShouldSetValueOfMissingLanguageAndVersion()
    {
      // act
      this.field.SetValue("en", 1, "v1");
      this.field.SetValue("en", 2, "v2");

      // assert
      this.field.Values["en"][1].Should().Be("v1");
      this.field.Values["en"][2].Should().Be("v2");
    }

    [Theory]
    [InlineData("__Created", true)]
    [InlineData("Title", false)]
    public void ShouldBeStandardIfNameStartsWithDashes(string name, bool standard)
    {
      // act & assert
      new DbField(name).IsStandard().Should().Be(standard);
    }

    [Theory, AutoData]
    public void ShouldGetEmptyValueForInvariantLanguageIfNotShared(DbField sut)
    {
      // arrange
      sut.Shared = false;

      // act & assert
      sut.GetValue(Language.Invariant.Name, 0).Should().BeEmpty();
    }

    [Theory, AutoData]
    public void ShouldGetSomeValueForInvariantLanguageIfShared(DbField sut)
    {
      // arrange
      sut.Shared = true;

      // act & assert
      sut.GetValue(Language.Invariant.Name, 0).Should().NotBeEmpty();
    }

    [Theory]
    [InlineAutoData(true)]
    [InlineAutoData(false)]
    public void SetValueTwiceResetsExistingValue(bool shared, DbField sut, string oldValue, string newValue)
    {
      // arrange
      sut.Shared = shared;
      sut.SetValue("en", oldValue);

      // act
      sut.SetValue("en", newValue);

      // assert
      sut.Value.Should().Be(newValue);
    }

    [Theory]
    [InlineAutoData("__Renderings", true)]
    [InlineAutoData("__Final Renderings", false)]
    public void ShouldSetFieldShared(string fieldName, bool shared)
    {
      new DbField(fieldName).Shared.Should().Be(shared);
    }

    [Fact]
    public void ShouldSetFieldType()
    {
      new DbField("__Renderings").Type.Should().Be("layout");
    }

    [Theory, AutoData]
    public void GetValueThrowsIfLanguageIsNull([NoAutoProperties] DbField sut, int version)
    {
      Action action = () => sut.GetValue(null, version);
      action.ShouldThrow<ArgumentNullException>().WithMessage("*language");
    }

    [Theory, AutoData]
    public void SetValueThrowsIfLanguageIsNull([NoAutoProperties] DbField sut)
    {
      Action action = () => sut.SetValue(null, null);
      action.ShouldThrow<ArgumentNullException>().WithMessage("*language");
    }

    [Theory, AutoData]
    public void ShouldUpdateAllValuesForSharedField([NoAutoProperties] DbField sut, string value1, string expected)
    {
      sut.Shared = true;
      sut.Add("en", 1, value1);
      sut.Add("en", 2, expected);

      sut.Values["en"][1].Should().Be(expected);
      sut.Values["en"][2].Should().Be(expected);
    }
  }
}