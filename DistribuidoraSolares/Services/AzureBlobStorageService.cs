using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace DistribuidoraSolares.Services;

public interface IAzureBlobStorageService
{
    Task<string> SubirImagenAsync(Stream archivoStream, string nombreArchivo, string contentType);
    Task<bool> EliminarImagenAsync(string nombreArchivo);
    Task<bool> ExisteImagenAsync(string nombreArchivo);
    string ObtenerUrlImagen(string nombreArchivo);
}

public class AzureBlobStorageService : IAzureBlobStorageService
{
    private readonly BlobServiceClient _blobServiceClient;
    private readonly string _containerName;
    private readonly string _connectionString;

    public AzureBlobStorageService(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("AzureStorage") 
            ?? throw new InvalidOperationException("AzureStorage connection string no configurada");
        
        _containerName = configuration["AzureStorage:ContainerName"] ?? "productos-imagenes";
        
        _blobServiceClient = new BlobServiceClient(_connectionString);
        
        // Crear el contenedor si no existe
        InitializeContainerAsync().GetAwaiter().GetResult();
    }

    private async Task InitializeContainerAsync()
    {
        var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
        await containerClient.CreateIfNotExistsAsync(PublicAccessType.Blob);
    }

    public async Task<string> SubirImagenAsync(Stream archivoStream, string nombreArchivo, string contentType)
    {
        try
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
            var blobClient = containerClient.GetBlobClient(nombreArchivo);

            // Configurar opciones de carga
            var uploadOptions = new BlobUploadOptions
            {
                HttpHeaders = new BlobHttpHeaders
                {
                    ContentType = contentType
                }
            };

            // Subir el archivo
            await blobClient.UploadAsync(archivoStream, uploadOptions);

            // Retornar la URL pública del blob
            return blobClient.Uri.ToString();
        }
        catch (Exception ex)
        {
            throw new Exception($"Error al subir imagen a Azure Blob Storage: {ex.Message}", ex);
        }
    }

    public async Task<bool> EliminarImagenAsync(string nombreArchivo)
    {
        try
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
            
            // Si la URL completa viene, extraer solo el nombre del archivo
            string blobName = nombreArchivo;
            if (nombreArchivo.StartsWith("http://") || nombreArchivo.StartsWith("https://"))
            {
                try
                {
                    var uri = new Uri(nombreArchivo);
                    // Extraer la ruta después del nombre del contenedor
                    var segments = uri.Segments.ToList();
                    var containerIndex = segments.FindIndex(s => s.TrimEnd('/') == _containerName);
                    if (containerIndex >= 0 && containerIndex < segments.Count - 1)
                    {
                        blobName = string.Join("", segments.Skip(containerIndex + 1));
                    }
                    else
                    {
                        // Fallback: usar el último segmento
                        blobName = segments.Last();
                    }
                }
                catch
                {
                    // Si falla el parsing, intentar usar el nombre completo
                    blobName = nombreArchivo;
                }
            }

            var blobClient = containerClient.GetBlobClient(blobName);
            var response = await blobClient.DeleteIfExistsAsync();
            return response.Value;
        }
        catch (Exception ex)
        {
            throw new Exception($"Error al eliminar imagen de Azure Blob Storage: {ex.Message}", ex);
        }
    }

    public async Task<bool> ExisteImagenAsync(string nombreArchivo)
    {
        try
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
            
            // Si la URL completa viene, extraer solo el nombre del archivo
            if (nombreArchivo.Contains("/"))
            {
                var uri = new Uri(nombreArchivo);
                nombreArchivo = uri.Segments.Last();
            }

            var blobClient = containerClient.GetBlobClient(nombreArchivo);
            return await blobClient.ExistsAsync();
        }
        catch
        {
            return false;
        }
    }

    public string ObtenerUrlImagen(string nombreArchivo)
    {
        try
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
            
            // Si la URL completa viene, retornarla directamente
            if (nombreArchivo.StartsWith("http://") || nombreArchivo.StartsWith("https://"))
            {
                return nombreArchivo;
            }

            // Si solo viene el nombre del archivo, construir la URL
            var blobClient = containerClient.GetBlobClient(nombreArchivo);
            return blobClient.Uri.ToString();
        }
        catch
        {
            return nombreArchivo;
        }
    }
}
