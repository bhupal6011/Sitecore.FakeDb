﻿namespace Sitecore.FakeDb.Tests.Security.Accounts
{
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using FluentAssertions;
  using NSubstitute;
  using Ploeh.AutoFixture.Xunit2;
  using Sitecore.FakeDb.Security.Accounts;
  using Sitecore.Reflection;
  using Sitecore.Security.Accounts;
  using Sitecore.Security.Domains;
  using Xunit;

  public class FakeRolesInRolesProviderTest : IDisposable
  {
    private const string RoleName = @"sitecore\Editors";

    private const string UserName = @"sitecore\John";

    private readonly FakeRolesInRolesProvider provider;

    private readonly RolesInRolesProvider localProvider;

    private readonly IEnumerable<Role> targetRoles = new List<Role>();

    private readonly IEnumerable<Role> resultRoles = new List<Role>();

    private readonly IEnumerable<Role> memberRoles = new List<Role>();

    private readonly Role role = Role.FromName("Role");

    private readonly Role targetRole = Role.FromName("Target Role");

    private readonly Role memberRole = Role.FromName("Member Role");

    private readonly Role resultRole = Role.FromName("Result Role");

    private readonly User user = User.FromName("User", true);

    public FakeRolesInRolesProviderTest()
    {
      this.localProvider = Substitute.For<RolesInRolesProvider>();

      this.provider = new FakeRolesInRolesProvider();
      this.provider.LocalProvider.Value = this.localProvider;
    }

    [Fact]
    public void ShouldNotThrowIfNoBefaviourSet()
    {
      // arrange
      var stubProvider = new FakeRolesInRolesProvider();

      // act & assert
      stubProvider.AddRolesToRoles(null, null);
      stubProvider.FindRolesInRole(null, null, false);
      stubProvider.FindUsersInRole(null, null, false);
      stubProvider.GetAllRoles(false);
      stubProvider.GetCreatorOwnerRole();
      stubProvider.GetEveryoneRole();
      stubProvider.GetEveryoneRoles();
      stubProvider.GetGlobalRoles();
      stubProvider.GetRoleMembers(null, true);
      stubProvider.GetRolesForRole(null, true);
      stubProvider.GetRolesForUser(null, true);
      stubProvider.GetRolesInRole(null, true);
      stubProvider.GetSystemRoles();
      stubProvider.GetUsersInRole(null, true);
      stubProvider.IsCreatorOwnerRole(null);
      stubProvider.IsEveryoneRole("Everyone");
      stubProvider.IsEveryoneRole("Everyone", Domain.GetDomain("exranet"));
      stubProvider.IsGlobalRole(null);
      stubProvider.IsRoleInRole(null, null, true);
      stubProvider.IsSystemRole(null);
      stubProvider.IsUserInRole(null, null, true);
      stubProvider.RemoveRoleRelations(null);
      stubProvider.RemoveRolesFromRoles(null, null);
    }

    [Fact]
    public void ShouldGetDefaultValuesIfNoBefaviourSet()
    {
      // arrange
      var stubProvider = new FakeRolesInRolesProvider();

      // act & assert
      stubProvider.FindRolesInRole(null, null, false).Should().BeEmpty();
      stubProvider.FindUsersInRole(null, null, false).Should().BeEmpty();
      stubProvider.GetAllRoles(false).Should().BeEmpty();
      stubProvider.GetCreatorOwnerRole().Should().Be(Role.FromName("Creator-Owner"));
      stubProvider.GetEveryoneRole().Should().Be(Role.FromName("Everyone"));
      stubProvider.GetEveryoneRoles().Should().BeEmpty();
      stubProvider.GetGlobalRoles().Should().BeEmpty();
      stubProvider.GetRoleMembers(null, true).Should().BeEmpty();
      stubProvider.GetRolesForRole(null, true).Should().BeEmpty();
      stubProvider.GetRolesForUser(null, true).Should().BeEmpty();
      stubProvider.GetRolesInRole(null, true).Should().BeEmpty();
      stubProvider.GetSystemRoles().Should().BeEmpty();
      stubProvider.GetUsersInRole(null, true).Should().BeEmpty();
      stubProvider.IsCreatorOwnerRole(null).Should().BeFalse();
      stubProvider.IsEveryoneRole("Everyone").Should().BeTrue();
      stubProvider.IsEveryoneRole("Everyone", Domain.GetDomain("extranet")).Should().BeTrue();
      stubProvider.IsGlobalRole(null).Should().BeFalse();
      stubProvider.IsRoleInRole(null, null, true).Should().BeFalse();
      stubProvider.IsSystemRole(null).Should().BeFalse();
      stubProvider.IsUserInRole(null, null, true).Should().BeFalse();
    }

    [Theory]
    [InlineAutoData("Everyone", true)]
    [InlineAutoData("Somebody", false)]
    public void ShouldCheckIfEveryoneRole(string roleName, bool expectedResult, FakeRolesInRolesProvider sut)
    {
      sut.IsEveryoneRole(roleName).Should().Be(expectedResult);
    }

    [Theory]
    [InlineData(@"extranet\Everyone", true)]
    [InlineData(@"extranet\Somebody", false)]
    public void ShouldCheckIfDomainEveryoneRole(string roleName, bool expectedResult)
    {
      var stubProvider = new FakeRolesInRolesProvider();
      var domain = Domain.GetDomain("extranet");

      stubProvider.IsEveryoneRole(roleName, domain).Should().Be(expectedResult);
    }

    [Fact]
    public void ShouldBeThreadLocalProvider()
    {
      this.provider.Should().BeAssignableTo<IThreadLocalProvider<RolesInRolesProvider>>();
    }

    [Fact]
    public void ShouldAddRolesToRoles()
    {
      this.provider.AddRolesToRoles(this.memberRoles, this.targetRoles);
      this.localProvider.Received().AddRolesToRoles(this.memberRoles, this.targetRoles);
    }

    [Fact]
    public void ShouldFindRolesInRole()
    {
      this.localProvider.FindRolesInRole(this.targetRole, RoleName, true).Returns(this.resultRoles);
      this.provider.FindRolesInRole(this.targetRole, RoleName, true).Should().BeSameAs(this.resultRoles);
    }

    [Theory, AutoData]
    public void ShouldFindRolesInRoleByTargetRoleAndRoleToMatch(string targetRole, string roleToMatch)
    {
      this.localProvider.FindRolesInRole(this.targetRole, RoleName, true).Returns(this.resultRoles);
      ReflectionUtil.CallMethod(this.provider, "FindRolesInRole", new object[] { targetRole, roleToMatch }).Should().Be(Enumerable.Empty<Role>());
    }

    [Fact]
    public void ShouldFindUsersInRole()
    {
      this.localProvider.FindUsersInRole(this.targetRole, UserName, true).Returns(this.resultRoles);
      this.provider.FindUsersInRole(this.targetRole, UserName, true).Should().BeSameAs(this.resultRoles);
    }

    [Fact]
    public void ShouldGetAllRoles()
    {
      this.localProvider.GetAllRoles(true).Returns(this.resultRoles);
      this.provider.GetAllRoles(true).Should().BeSameAs(this.resultRoles);
    }

    [Fact]
    public void ShouldGetCreatorOwnerRole()
    {
      this.localProvider.GetCreatorOwnerRole().Returns(this.resultRole);
      this.provider.GetCreatorOwnerRole().Should().BeSameAs(this.resultRole);
    }

    [Fact]
    public void ShouldGetEveryoneRole()
    {
      this.localProvider.GetEveryoneRole().Returns(this.resultRole);
      this.provider.GetEveryoneRole().Should().BeSameAs(this.resultRole);
    }

    [Theory, AutoData]
    public void ShouldGetEveryoneRoleForDomain(Domain domain)
    {
      this.localProvider.GetEveryoneRole(domain).Returns(this.resultRole);
      this.provider.GetEveryoneRole(domain).Should().BeSameAs(this.resultRole);
    }

    [Fact]
    public void ShouldGetEveryoneRoles()
    {
      this.localProvider.GetEveryoneRoles().Returns(this.resultRoles);
      this.provider.GetEveryoneRoles().Should().BeSameAs(this.resultRoles);
    }

    [Fact]
    public void ShouldGetGlobalRoles()
    {
      this.localProvider.GetGlobalRoles().Returns(this.resultRoles);
      this.provider.GetGlobalRoles().Should().BeSameAs(this.resultRoles);
    }

    [Fact]
    public void ShouldGetRoleMembers()
    {
      this.localProvider.GetRoleMembers(this.role, true).Returns(this.resultRoles);
      this.provider.GetRoleMembers(this.role, true).Should().BeSameAs(this.resultRoles);
    }

    [Fact]
    public void ShouldGetRolesForRole()
    {
      this.localProvider.GetRolesForRole(this.role, true).Returns(this.resultRoles);
      this.provider.GetRolesForRole(this.role, true).Should().BeSameAs(this.resultRoles);
    }

    [Theory, AutoData]
    public void ShouldGetRolesForRoleByTargeRole(string targetRole)
    {
      this.localProvider.FindRolesInRole(this.targetRole, RoleName, true).Returns(this.resultRoles);
      ReflectionUtil.CallMethod(this.provider, "GetRolesForRole", new object[] { targetRole }).Should().Be(Enumerable.Empty<Role>());
    }

    [Fact]
    public void ShouldGetRolesForUser()
    {
      this.localProvider.GetRolesForUser(this.user, true).Returns(this.resultRoles);
      this.provider.GetRolesForUser(this.user, true).Should().BeSameAs(this.resultRoles);
    }

    [Fact]
    public void ShouldGetRolesInRole()
    {
      this.localProvider.GetRolesInRole(this.role, true).Returns(this.resultRoles);
      this.provider.GetRolesInRole(this.role, true).Should().BeSameAs(this.resultRoles);
    }

    [Theory, AutoData]
    public void ShouldGetRolesInRoleByTargeRole(string targetRole)
    {
      this.localProvider.FindRolesInRole(this.targetRole, RoleName, true).Returns(this.resultRoles);
      ReflectionUtil.CallMethod(this.provider, "GetRolesInRole", new object[] { targetRole }).Should().Be(Enumerable.Empty<Role>());
    }

    [Fact]
    public void ShouldGetSystemRoles()
    {
      this.localProvider.GetSystemRoles().Returns(this.resultRoles);
      this.provider.GetSystemRoles().Should().BeSameAs(this.resultRoles);
    }

    [Theory, AutoData]
    public void ShouldGetUsersInRole(List<User> users)
    {
      this.localProvider.GetUsersInRole(this.role, true).Returns(users);
      this.provider.GetUsersInRole(this.role, true).Should().BeSameAs(users);
    }

    [Fact]
    public void ShouldCallIsCreatorOwnerRole()
    {
      this.localProvider.IsCreatorOwnerRole(RoleName).Returns(true);
      this.provider.IsCreatorOwnerRole(RoleName).Should().BeTrue();
    }

    [Fact]
    public void ShouldCallIsEveryoneRole()
    {
      this.localProvider.IsEveryoneRole(RoleName).Returns(true);
      this.provider.IsEveryoneRole(RoleName).Should().BeTrue();
    }

    [Theory, AutoData]
    public void ShouldCallIsEveryoneRoleWithDomain(Domain domain)
    {
      this.localProvider.IsEveryoneRole(RoleName, domain).Returns(true);
      this.provider.IsEveryoneRole(RoleName, domain).Should().BeTrue();
    }

    [Fact]
    public void ShouldCallIsGlobalRole()
    {
      this.localProvider.IsGlobalRole(this.role).Returns(true);
      this.provider.IsGlobalRole(this.role).Should().BeTrue();
    }

    [Fact]
    public void ShouldCallIsRoleInRole()
    {
      this.localProvider.IsRoleInRole(this.memberRole, this.targetRole, true).Returns(true);
      this.provider.IsRoleInRole(this.memberRole, this.targetRole, true).Should().BeTrue();
    }

    [Theory, AutoData]
    public void ShouldCallIsRoleInRoleWithMemberAndTargetRoles(string memberRole, string targetRole)
    {
      this.localProvider.FindRolesInRole(this.targetRole, RoleName, true).Returns(this.resultRoles);
      ReflectionUtil.CallMethod(this.provider, "IsRoleInRole", new object[] { memberRole, targetRole }).Should().Be(false);
    }

    [Fact]
    public void ShouldCallIsSystemRole()
    {
      this.localProvider.IsSystemRole(RoleName).Returns(true);
      this.provider.IsSystemRole(RoleName).Should().BeTrue();
    }

    [Fact]
    public void ShouldCallIsUserInRole()
    {
      this.localProvider.IsUserInRole(this.user, this.targetRole, true).Returns(true);
      this.provider.IsUserInRole(this.user, this.targetRole, true).Should().BeTrue();
    }

    [Fact]
    public void ShouldCallRemoveRoleRelations()
    {
      this.provider.RemoveRoleRelations(RoleName);
      this.localProvider.Received().RemoveRoleRelations(RoleName);
    }

    [Fact]
    public void ShouldCallRemoveRolesFromRoles()
    {
      this.provider.RemoveRolesFromRoles(this.memberRoles, this.targetRoles);
      this.localProvider.Received().RemoveRolesFromRoles(this.memberRoles, this.targetRoles);
    }

    public void Dispose()
    {
      this.provider.Dispose();
    }
  }
}
