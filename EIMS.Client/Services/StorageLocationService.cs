using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using EIMS.Shared.Models;

namespace EIMS.Client.Services
{
    /// <summary>
    /// Client service for managing storage locations and generating new locations
    /// </summary>
    public class StorageLocationService
    {
        private readonly HttpClient _httpClient;

        public StorageLocationService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        #region Storage Location CRUD

        /// <summary>
        /// Gets all storage locations
        /// </summary>
        public async Task<List<StorageLocation>> GetLocationsAsync()
        {
            try
            {
                var response = await _httpClient.GetFromJsonAsync<List<StorageLocation>>("api/StorageLocations");
                return response ?? new List<StorageLocation>();
            }
            catch (Exception)
            {
                // Handle or log error
                return new List<StorageLocation>();
            }
        }

        /// <summary>
        /// Gets a specific storage location by ID
        /// </summary>
        public async Task<StorageLocation?> GetLocationAsync(int id)
        {
            try
            {
                return await _httpClient.GetFromJsonAsync<StorageLocation>($"api/StorageLocations/{id}");
            }
            catch (Exception)
            {
                // Handle or log error
                return null;
            }
        }

        /// <summary>
        /// Creates a new storage location
        /// </summary>
        public async Task<(bool Success, StorageLocation? Location, string? ErrorMessage)> CreateLocationAsync(StorageLocation location)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/StorageLocations", location);
                
                if (response.IsSuccessStatusCode)
                {
                    var createdLocation = await response.Content.ReadFromJsonAsync<StorageLocation>();
                    return (true, createdLocation, null);
                }
                
                var errorMessage = await response.Content.ReadAsStringAsync();
                return (false, null, errorMessage);
            }
            catch (Exception ex)
            {
                return (false, null, ex.Message);
            }
        }

        /// <summary>
        /// Updates an existing storage location
        /// </summary>
        public async Task<(bool Success, string? ErrorMessage)> UpdateLocationAsync(StorageLocation location)
        {
            try
            {
                var response = await _httpClient.PutAsJsonAsync($"api/StorageLocations/{location.Id}", location);
                
                if (response.IsSuccessStatusCode)
                {
                    return (true, null);
                }
                
                var errorMessage = await response.Content.ReadAsStringAsync();
                return (false, errorMessage);
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }

        /// <summary>
        /// Assigns a part to a storage location
        /// </summary>
        public async Task<(bool Success, string? ErrorMessage)> AssignPartToLocationAsync(int locationId, int partId)
        {
            try
            {
                var response = await _httpClient.PutAsync($"api/StorageLocations/{locationId}/assignPart/{partId}", null);
                
                if (response.IsSuccessStatusCode)
                {
                    return (true, null);
                }
                
                var errorMessage = await response.Content.ReadAsStringAsync();
                return (false, errorMessage);
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }

        #endregion

        #region Location Generation

        /// <summary>
        /// Generates a preview of locations based on the provided generation request
        /// </summary>
        public async Task<(bool Success, LocationGenerationPreview? Preview, string? ErrorMessage)> GeneratePreviewAsync(LocationGenerationRequest request)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/LocationGeneration/preview", request);
                
                if (response.IsSuccessStatusCode)
                {
                    var preview = await response.Content.ReadFromJsonAsync<LocationGenerationPreview>();
                    return (true, preview, null);
                }
                
                var errorMessage = await response.Content.ReadAsStringAsync();
                return (false, null, errorMessage);
            }
            catch (Exception ex)
            {
                return (false, null, ex.Message);
            }
        }

        /// <summary>
        /// Generates actual storage locations based on the provided generation request
        /// </summary>
        public async Task<(bool Success, string? ErrorMessage)> GenerateLocationsAsync(LocationGenerationRequest request)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/LocationGeneration/generate", request);
                
                if (response.IsSuccessStatusCode)
                {
                    return (true, null);
                }
                
                var errorMessage = await response.Content.ReadAsStringAsync();
                return (false, errorMessage);
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }
        
        #endregion
    }
} 