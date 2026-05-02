using Moq;
using Xunit;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using System.Text;
using AestheticClinicAPI.Shared;
using AestheticClinicAPI.Modules.Photos.Services;
using AestheticClinicAPI.Modules.Photos.Repositories;
using AestheticClinicAPI.Modules.Photos.Models;
using AestheticClinicAPI.Modules.Photos.DTOs;
using AestheticClinicAPI.Modules.Clients.Services;
using AestheticClinicAPI.Modules.Clients.Models;
using Microsoft.AspNetCore.Hosting;

namespace AestheticClinicAPI.Tests.UnitTests.Photos;

public class PhotoServiceTests
{
    private readonly Mock<IPhotoRepository> _photoRepoMock;
    private readonly Mock<IClientService> _clientServiceMock;
    private readonly Mock<IWebHostEnvironment> _envMock;
    private readonly PhotoService _photoService;
    private readonly string _testUploadsPath;

    public PhotoServiceTests()
    {
        _photoRepoMock = new Mock<IPhotoRepository>();
        _clientServiceMock = new Mock<IClientService>();
        _envMock = new Mock<IWebHostEnvironment>();
        
        // Setup a temporary directory for file upload tests
        _testUploadsPath = Path.Combine(Path.GetTempPath(), "test_uploads", "photos");
        _envMock.Setup(e => e.WebRootPath).Returns(Path.GetTempPath());
        
        _photoService = new PhotoService(
            _photoRepoMock.Object,
            _clientServiceMock.Object,
            _envMock.Object);
    }

    private ClientResponseDto CreateSampleClientDto() => new ClientResponseDto
    {
        Id = 1,
        FirstName = "Maria",
        LastName = "Santos",
    };

    private Photo CreateSamplePhoto(int id = 1) => new Photo
    {
        Id = id,
        ClientId = 1,
        AppointmentId = 10,
        FileName = "original.jpg",
        FilePath = $"/uploads/photos/{Guid.NewGuid()}.jpg",
        Description = "Before treatment",
        IsBefore = true,
        FileSize = 1024,
        MimeType = "image/jpeg",
        CreatedAt = DateTime.UtcNow
    };

    private IFormFile CreateMockIFormFile(string fileName = "test.jpg", string contentType = "image/jpeg")
    {
        var content = "fake image content";
        var stream = new MemoryStream(Encoding.UTF8.GetBytes(content));
        var formFile = new FormFile(stream, 0, stream.Length, "file", fileName)
        {
            Headers = new HeaderDictionary(),
            ContentType = contentType
        };
        return formFile;
    }

    [Fact]
    public async Task GetByIdAsync_ExistingPhoto_ReturnsDto()
    {
        // Arrange
        var photo = CreateSamplePhoto(1);
        _photoRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(photo);
        _clientServiceMock.Setup(c => c.GetByIdAsync(1))
            .ReturnsAsync(ServiceResult<ClientResponseDto>.Success(CreateSampleClientDto()));

        // Act
        var result = await _photoService.GetByIdAsync(1);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(1, result.Data?.Id);
        Assert.Equal("Maria Santos", result.Data?.ClientName);
        Assert.Equal("original.jpg", result.Data?.FileName);
        Assert.True(result.Data?.IsBefore);
    }

    [Fact]
    public async Task GetByIdAsync_NonExistingPhoto_ReturnsFailure()
    {
        // Arrange
        _photoRepoMock.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((Photo?)null);

        // Act
        var result = await _photoService.GetByIdAsync(99);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("Photo not found.", result.ErrorMessage);
    }

    [Fact]
    public async Task GetByClientAsync_ReturnsPhotos()
    {
        // Arrange
        var photos = new List<Photo> { CreateSamplePhoto(1), CreateSamplePhoto(2) };
        _photoRepoMock.Setup(r => r.GetByClientAsync(1)).ReturnsAsync(photos);
        _clientServiceMock.Setup(c => c.GetByIdAsync(1))
            .ReturnsAsync(ServiceResult<ClientResponseDto>.Success(CreateSampleClientDto()));

        // Act
        var result = await _photoService.GetByClientAsync(1);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Data?.Count());
    }

    [Fact]
    public async Task GetByAppointmentAsync_ReturnsPhotos()
    {
        // Arrange
        var photos = new List<Photo> { CreateSamplePhoto(1) };
        _photoRepoMock.Setup(r => r.GetByAppointmentAsync(10)).ReturnsAsync(photos);
        _clientServiceMock.Setup(c => c.GetByIdAsync(1))
            .ReturnsAsync(ServiceResult<ClientResponseDto>.Success(CreateSampleClientDto()));

        // Act
        var result = await _photoService.GetByAppointmentAsync(10);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Single(result.Data!);
    }

    [Fact]
    public async Task GetBeforePhotosAsync_ReturnsBeforePhotos()
    {
        // Arrange
        var photos = new List<Photo> { CreateSamplePhoto(1) };
        _photoRepoMock.Setup(r => r.GetBeforePhotosAsync(1)).ReturnsAsync(photos);
        _clientServiceMock.Setup(c => c.GetByIdAsync(1))
            .ReturnsAsync(ServiceResult<ClientResponseDto>.Success(CreateSampleClientDto()));

        // Act
        var result = await _photoService.GetBeforePhotosAsync(1);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Single(result.Data!);
        Assert.True(result.Data!.First().IsBefore);
    }

    [Fact]
    public async Task DeleteAsync_ExistingPhoto_DeletesFileAndRecord()
    {
        // Arrange
        var photo = CreateSamplePhoto(1);
        _photoRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(photo);
        _photoRepoMock.Setup(r => r.DeleteAsync(photo)).Returns(Task.CompletedTask);

        var wwwroot = _envMock.Object.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
        var physicalPath = Path.Combine(wwwroot, photo.FilePath.TrimStart('/'));
        Directory.CreateDirectory(Path.GetDirectoryName(physicalPath)!);
        await File.WriteAllTextAsync(physicalPath, "dummy");

        // Act
        var result = await _photoService.DeleteAsync(1);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.False(File.Exists(physicalPath));
        _photoRepoMock.Verify(r => r.DeleteAsync(photo), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_NonExistingPhoto_ReturnsFailure()
    {
        // Arrange
        _photoRepoMock.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((Photo?)null);

        // Act
        var result = await _photoService.DeleteAsync(99);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("Photo not found.", result.ErrorMessage);
    }

    [Fact]
    public async Task GetPhotoFileAsync_ExistingPhoto_ReturnsFileBytes()
    {
        // Arrange
        var photo = CreateSamplePhoto(1);
        _photoRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(photo);

        var wwwroot = _envMock.Object.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
        var physicalPath = Path.Combine(wwwroot, photo.FilePath.TrimStart('/'));
        Directory.CreateDirectory(Path.GetDirectoryName(physicalPath)!);
        await File.WriteAllTextAsync(physicalPath, "test file content");

        // Act
        var result = await _photoService.GetPhotoFileAsync(1);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Equal("test file content", Encoding.UTF8.GetString(result.Data.FileBytes));
        Assert.Equal("image/jpeg", result.Data.MimeType);
        Assert.Equal("original.jpg", result.Data.FileName);
    }

    [Fact]
    public async Task GetPhotoFileAsync_FileMissing_ReturnsFailure()
    {
        // Arrange
        var photo = CreateSamplePhoto(1);
        _photoRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(photo);
        // Ensure file does not exist

        // Act
        var result = await _photoService.GetPhotoFileAsync(1);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("File not found on disk.", result.ErrorMessage);
    }
}