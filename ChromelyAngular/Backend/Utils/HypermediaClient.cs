using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

using ChromelyAngular.Backend.Models.JsonApi;

using Hypermedia.Configuration;
using Hypermedia.JsonApi;
using Hypermedia.JsonApi.Client;

namespace ChromelyAngular.Backend.Utils
{
    public class HypermediaClient
    {
        private const string MediaTypeName = "application/vnd.api+json";

        private readonly static HttpClientHandler clientHandler = new HttpClientHandler()
        {
            ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true
        };

        private readonly static HttpClient _httpClient = new HttpClient(clientHandler) { BaseAddress = new Uri("https://sks-test.arena-expert.ru:44443") };

        private readonly Hypermedia.Metadata.IContractResolver _contractResolver;

        private readonly IJsonApiEntityCache _cache = new JsonApiEntityCache();

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="endpoint">The endpoint to connect the client to.</param>
        /// <param name="accessToken">The access token.</param>
        public HypermediaClient()
        {
            //_httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(MediaTypeName));
            _contractResolver = CreateResolver();
        }

        /// <summary>
        /// Returns the resource contract resolver for the known types.
        /// </summary>
        /// <returns>The resource contract resolver to use when serializing the types.</returns>
        public static Hypermedia.Metadata.IContractResolver CreateResolver()
        {
            return new Builder()
                .With<Position>("positions").Id(nameof(Position.Id))
                .With<Area>("areas").Id(nameof(Area.Id))
                .With<Place>("places").Id(nameof(Place.Id))
                .With<BlockReason>("blockReasons").Id(nameof(BlockReason.Id))
                .With<Tournament>("tournaments").Id(nameof(Tournament.Id))
                .With<Models.JsonApi.Event>("events").Id(nameof(Models.JsonApi.Event.Id))
                .Build();
        }

        /// <summary>
        /// Returns a list of T.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <param name="skip">The number of users to skip from the start.</param>
        /// <param name="take">The number of users to return.</param>
        /// <returns>The list of T.</returns>
        public Task<IReadOnlyList<Position>> GetPositionsAsync(int page = 1, int size = 10, string sort = "name", CancellationToken cancellationToken = default)
            => GetAsync<Position>("positions", page, size, sort, cancellationToken);

        public Task<IReadOnlyList<Area>> GetAreasAsync(int page = 1, int size = 10, string sort = "name", CancellationToken cancellationToken = default)
            => GetAsync<Area>("areas", page, size, sort, cancellationToken);

        public Task<IReadOnlyList<Place>> GetPlacesAsync(int page = 1, int size = 10, string sort = "name", CancellationToken cancellationToken = default)
            => GetAsync<Place>("places", page, size, sort, cancellationToken);

        public Task<IReadOnlyList<BlockReason>> GetBlockReasonsAsync(int page = 1, int size = 10, string sort = "name", CancellationToken cancellationToken = default)
            => GetAsync<BlockReason>("blockReasons", page, size, sort, cancellationToken);

        public Task<IReadOnlyList<Tournament>> GetTournamentsAsync(int page = 1, int size = 10, string sort = "name", CancellationToken cancellationToken = default)
            => GetAsync<Tournament>("tournaments", page, size, sort, cancellationToken);

        public async Task<IReadOnlyList<Models.JsonApi.Event>> GetEventsAsync(int page = 1, int size = 10, string sort = "name", CancellationToken cancellationToken = default)
            => await GetAsync<Models.JsonApi.Event>("events", page, size, sort, cancellationToken).ConfigureAwait(false);

        public async Task<IReadOnlyList<T>> GetAsync<T>(string source, int page = 1, int size = 10, string sort = "name", CancellationToken cancellationToken = default)
        {
            var response = await _httpClient.GetAsync($"api/v1/{source}?sort={sort}&page[size]={size}&page[number]={page}", cancellationToken).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsJsonApiManyAsync<T>(_contractResolver, _cache).ConfigureAwait(false);
        }

        ///// <summary>
        ///// Return the post with the given resource ID.
        ///// </summary>
        ///// <param name="id">The ID of the post to return.</param>
        ///// <param name="cancellationToken">The cancellation token.</param>
        ///// <returns>The post with the given resource id.</returns>
        //public async Task<PostResource> GetPostByIdAsync(int id, CancellationToken cancellationToken = default)
        //{
        //    var response = await _httpClient.GetAsync($"v1/posts/{id}", cancellationToken).ConfigureAwait(false);
        //    response.EnsureSuccessStatusCode();

        //    return await response.Content.ReadAsJsonApiAsync<PostResource>(_contractResolver, _cache).ConfigureAwait(false);
        //}

        ///// <summary>
        ///// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        ///// </summary>
        //public void Dispose()
        //{
        //    _httpClient.Dispose();
        //}
    }
}
