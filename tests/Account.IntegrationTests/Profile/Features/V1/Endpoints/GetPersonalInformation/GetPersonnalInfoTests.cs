using System;
using System.Net.Http;
using System.Threading.Tasks;
using Account.Data.Models;
using Account.IntegrationTests.Common;
using Account.IntegrationTests.Extensions;
using Account.Profile.Features.V1.GetPersonalInformation;
using Account.Profile.Models;

namespace Account.IntegrationTests.Profile.Features.V1.Endpoints.GetPersonalInformation;

[Collection(nameof(ProfileApiCollection))]
[Trait("Category", "PersonalInformation")]
public sealed class GetPersonalInformationTests(ProfileWebAppFactory factory)
    : IntegrationTest(factory)
{
    [Theory]
    [MemberData(
        nameof(TestDataFactory.ValidProfileTestCases),
        MemberType = typeof(TestDataFactory)
    )]
    public async Task GetPersonalInformation_WithValidProfile_ShouldReturnExpectedData(
        ProfileTestCase testCase
    )
    {
        // Arrange
        ProfileReadModel profile = new ProfileTestDataBuilder()
            .WithBasicInfo(testCase)
            .WithAddress("123 Main St", "Paris", "75001", "France")
            .WithGender(Gender.Male)
            .Build();

        ProfileReadModel seededProfile = await this.Seeder.SeedProfile(profile);

        // Act
        (
            HttpResponseMessage Response,
            GetPersonalInformationEndpoint.PersonalInformationResponse? Content
        ) result = await this.Client.GetPersonalInformation(seededProfile.Id);

        // Assert
        await result.ShouldReturnValidProfileAsync(profile);
    }

    [Theory]
    [MemberData(
        nameof(TestDataFactory.AddressFormatTestCases),
        MemberType = typeof(TestDataFactory)
    )]
    public async Task GetPersonalInformation_WithPartialAddress_ShouldFormatAddressCorrectly(
        string? city,
        string? country,
        string expectedAddress
    )
    {
        // Arrange
        ProfileReadModel profile = new ProfileTestDataBuilder()
            .WithBasicInfo(new ProfileTestCase("Test", "User", "test@example.com", "+33612345678"))
            .Build();

        profile.City = city;
        profile.Country = country;

        ProfileReadModel seededProfile = await this.Seeder.SeedProfile(profile);

        // Act
        (
            HttpResponseMessage Response,
            GetPersonalInformationEndpoint.PersonalInformationResponse? Content
        ) result = await this.Client.GetPersonalInformation(seededProfile.Id);

        // Assert
        await result.ShouldReturnPartialAddressAsync(city, country, expectedAddress);
    }

    [Fact]
    public async Task GetPersonalInformation_WithPrefilledProfile_ShouldReturnCorrectData()
    {
        // Arrange
        ProfileReadModel seededProfile = await this.Seeder.SeedProfile(
            TestDataFactory.CreateDefaultProfile()
        );

        // Act
        (
            HttpResponseMessage Response,
            GetPersonalInformationEndpoint.PersonalInformationResponse? Content
        ) result = await this.Client.GetPersonalInformation(seededProfile.Id);

        // Assert
        await result.ShouldReturnValidProfileAsync(seededProfile);
    }

    [Fact]
    public async Task GetPersonalInformation_WithNonExistentId_ShouldReturnNotFound()
    {
        // Arrange
        Guid nonExistentId = Guid.NewGuid();

        // Act
        (HttpResponseMessage response, _) = await this.Client.GetPersonalInformation(nonExistentId);

        // Assert
        await response.ShouldBeNotFoundAsync($"Profile with ID {nonExistentId} was not found.");
    }
}
