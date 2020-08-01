using System;
using System.ComponentModel;

namespace SpotifyNet
{
    /// <summary>
    /// Scopes provide Spotify users using third-party apps the confidence that only the 
    /// information they choose to share will be shared, and nothing more.
    /// </summary>
    [Flags]
    public enum Scope : uint
    {
        /// <summary>
        /// No scopes
        /// </summary>
        None = 0,

        /// <summary>
        /// Read access to user’s subscription details (type of user account).
        /// </summary>
        [Description("user-read-private")]
        UserReadPrivate = 1,

        /// <summary>
        /// Read access to user’s email address.
        /// </summary>
        [Description("user-read-email")]
        UserReadEmail = 2 << 0,

        /// <summary>
        /// Read access to user’s birthdate.
        /// </summary>
        [Obsolete("Does not exist anymore")]
        [Description("user-read-birthdate")]
        UserReadBirthday = 2 << 1,

        /// <summary>
        /// Read access to user's private playlists.
        /// </summary>
        [Description("playlist-read-private")]
        PlaylistReadPrivate = 2 << 2,

        /// <summary>
        /// Write access to a user's private playlists.
        /// </summary>
        [Description("playlist-modify-private")]
        PlaylistModifyPrivate = 2 << 3,

        /// <summary>
        /// Write access to a user's public playlists.
        /// </summary>
        [Description("playlist-modify-public")]
        PlaylistModifyPublic = 2 << 4,

        /// <summary>
        /// Include collaborative playlists when requesting a user's playlists.
        /// </summary>
        [Description("playlist-read-collaborative")]
        PlaylistReadCollaborative = 2 << 5,

        /// <summary>
        /// Read access to a user's top artists and tracks.
        /// </summary>
        [Description("user-top-read")]
        UserTopRead = 2 << 6,

        /// <summary>
        /// Read access to a user’s recently played tracks.
        /// </summary>
        [Description("user-read-recently-played")]
        UserReadRecentlyPlayed = 2 << 7,

        /// <summary>
        /// Read access to a user's "Your Music" library.
        /// </summary>
        [Description("user-library-read")]
        UserLibraryRead = 2 << 8,

        /// <summary>
        /// Write/delete access to a user's "Your Music" library.
        /// </summary>
        [Description("user-library-modify")]
        UserLibraryModify = 2 << 9,

        /// <summary>
        /// Read access to a user’s currently playing content.
        /// </summary>
        [Description("user-read-currently-playing")]
        UserReadCurrentlyPlaying = 2 << 10,

        /// <summary>
        /// Write access to a user’s playback state.
        /// </summary>
        [Description("user-modify-playback-state")]
        UserModifyPlaybackState = 2 << 11,

        /// <summary>
        /// Read access to a user’s player state.
        /// </summary>
        [Description("user-read-playback-state")]
        UserReadPlaybackState = 2 << 12,

        /// <summary>
        /// Write/delete access to the list of artists and other users that the user follows.
        /// </summary>
        [Description("user-follow-modify")]
        UserFollowModify = 2 << 13,

        /// <summary>
        /// Read access to the list of artists and other users that the user follows.
        /// </summary>
        [Description("user-follow-read")]
        UserFollowRead = 2 << 14,

        /// <summary>
        /// Control playback of a Spotify track. This scope is currently available to the Web Playback SDK.
        /// The user must have a Spotify Premium account.
        /// </summary>
        [Description("streaming")]
        Streaming = 2 << 15,

        /// <summary>
        /// All scopes which are inside <see cref="Scope"/>.
        /// </summary>
        All = ~0u
    }
}
