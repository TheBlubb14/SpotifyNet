using System;
using System.ComponentModel;

namespace SpotifyNet
{
    [Flags]
    public enum Scope : uint
    {
        //None = 0,

        All = ~0u,

        [Description("user-read-private")]
        UserReadPrivate = 1,

        [Description("user-read-email")]
        UserReadEmail = 2 << 0,

        [Description("user-read-birthdate")]
        UserReadBirthday = 2 << 1,

        [Description("playlist-read-private")]
        PlaylistReadPrivate = 2 << 2,

        [Description("playlist-modify-private")]
        PlaylistModifyPrivate = 2 << 3,

        [Description("playlist-modify-public")]
        PlaylistModifyPublic = 2 << 4,

        [Description("playlist-read-collaborative")]
        PlaylistReadCollaborative = 2 << 5,

        [Description("user-top-read")]
        UserTopRead = 2 << 6,

        [Description("user-read-recently-played")]
        UserReadRecentlyPlayed = 2 << 7,

        [Description("user-library-read")]
        UserLibraryRead = 2 << 8,

        [Description("user-library-modify")]
        UserLibraryModify = 2 << 9,

        [Description("user-read-currently-playing")]
        UserReadCurrentlyPlaying = 2 << 10,

        [Description("user-modify-playback-state")]
        UserModifyPlaybackState = 2 << 11,

        [Description("user-read-playback-state")]
        UserReadPlaybackState = 2 << 12,

        [Description("user-follow-modify")]
        UserFollowModify = 2 << 13,

        [Description("user-follow-read")]
        UserFollowRead = 2 << 14,

        [Description("streaming")]
        Streaming = 2 << 15
    }
}
