using Aetherphone.Core.Aethernet.Contracts;

namespace Aetherphone.Apps.KindKupo;

internal static class KindKupoMockData
{
    // --- Lorem Ipsum Samples of Varying Lengths ---
    public const string LoremOneLiner = "Lorem ipsum dolor sit amet, consectetur adipiscing elit.";

    public const string LoremShort =
        "Sed do eiusmod tempor incididunt ut labore et dolore magna aliqua.";

    public const string LoremMedium =
        "Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate.";

    public const string LoremLong =
        "Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum. Curabitur pretium tincidunt lacus.";

    public const string LoremMultiParagraph =
        "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Integer nec odio. Praesent libero. Sed cursus ante dapibus diam. Sed nisi. Nulla quis sem at nibh elementum imperdiet.\n\nDuis sagittis ipsum. Praesent mauris. Fusce nec tellus sed augue semper porta. Mauris massa. Vestibulum lacinia arcu eget nulla. Class aptent taciti sociosqu ad litora torquent per conubia nostra, per inceptos himenaeos.";

    public static KindKupoStatsDto GetStats() => new(
        WrittenCount: 14,
        ResponseCount: 42,
        KudosCount: 88
    );

    public static List<ConfessionDto> GetConfessions()
    {
        var now = DateTime.UtcNow;
        return
        [
            new ConfessionDto(
                Id: "confession-1",
                AuthorId: "user-101",
                Text: LoremOneLiner,
                CreatedAt: now.AddMinutes(-15),
                ResponseCount: 1,
                Responses:
                [
                    new ResponseDto(
                        Id: "resp-101",
                        ConfessionId: "confession-1",
                        ResponderId: "user-201",
                        Text: LoremShort,
                        CreatedAt: now.AddMinutes(-10)
                    )
                ]
            ),
            new ConfessionDto(
                Id: "confession-2",
                AuthorId: "user-102",
                Text: LoremMedium,
                CreatedAt: now.AddHours(-2),
                ResponseCount: 3,
                Responses:
                [
                    new ResponseDto(
                        Id: "resp-201",
                        ConfessionId: "confession-2",
                        ResponderId: "user-301",
                        Text: LoremShort,
                        CreatedAt: now.AddHours(-1).AddMinutes(-40)
                    ),
                    new ResponseDto(
                        Id: "resp-202",
                        ConfessionId: "confession-2",
                        ResponderId: "user-302",
                        Text: LoremMedium,
                        CreatedAt: now.AddHours(-1).AddMinutes(-20)
                    ),
                    new ResponseDto(
                        Id: "resp-203",
                        ConfessionId: "confession-2",
                        ResponderId: "user-303",
                        Text: LoremOneLiner,
                        CreatedAt: now.AddMinutes(-50)
                    )
                ]
            ),
            new ConfessionDto(
                Id: "confession-3",
                AuthorId: "user-103",
                Text: LoremLong,
                CreatedAt: now.AddHours(-6),
                ResponseCount: 2,
                Responses:
                [
                    new ResponseDto(
                        Id: "resp-301",
                        ConfessionId: "confession-3",
                        ResponderId: "user-401",
                        Text: LoremMedium,
                        CreatedAt: now.AddHours(-4)
                    ),
                    new ResponseDto(
                        Id: "resp-302",
                        ConfessionId: "confession-3",
                        ResponderId: "user-402",
                        Text: LoremLong,
                        CreatedAt: now.AddHours(-3)
                    )
                ]
            ),
            new ConfessionDto(
                Id: "confession-4",
                AuthorId: "user-104",
                Text: LoremMultiParagraph,
                CreatedAt: now.AddDays(-1),
                ResponseCount: 0,
                Responses: []
            ),
            new ConfessionDto(
                Id: "confession-5",
                AuthorId: "user-105",
                Text: "Curabitur sodales ligula in libero. Sed dignissim lacinia nunc. Curabitur tortor. Pellentesque nibh. Aenean quam.",
                CreatedAt: now.AddDays(-2),
                ResponseCount: 1,
                Responses:
                [
                    new ResponseDto(
                        Id: "resp-501",
                        ConfessionId: "confession-5",
                        ResponderId: "user-501",
                        Text: LoremMultiParagraph,
                        CreatedAt: now.AddDays(-1).AddHours(-12)
                    )
                ]
            )
        ];
    }

    public static KindKupoInboxDto GetInbox(string accountId = "me") => new(
        AccountId: accountId,
        KupoInboxes: GetConfessions()
    );

    public static ConfessionDto CreateMockConfession(string content, string authorId = "me")
    {
        return new ConfessionDto(
            Id: "confession-" + Guid.NewGuid().ToString("N")[..8],
            AuthorId: authorId,
            Text: content,
            CreatedAt: DateTime.UtcNow,
            ResponseCount: 0,
            Responses: []
        );
    }

    public static ResponseDto CreateMockResponse(string confessionId, string content, string responderId = "me")
    {
        return new ResponseDto(
            Id: "resp-" + Guid.NewGuid().ToString("N")[..8],
            ConfessionId: confessionId,
            ResponderId: responderId,
            Text: content,
            CreatedAt: DateTime.UtcNow
        );
    }
}
