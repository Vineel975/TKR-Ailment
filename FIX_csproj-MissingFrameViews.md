# Fix — "Member View" / "Provider Details" tabs: view not found on the deployed server

**File:** `Enrollment/Enrollment.csproj`

## What's wrong
The `MemberViewFrame` and `ProviderDetailsFrame` controller actions run fine, but the deployed app can't find their `.cshtml` views. The files exist on disk and in your zip, but they are **not registered in `Enrollment.csproj`** as `<Content>` — unlike every other view (e.g. `Index.cshtml`).

- **Local works:** the MVC view engine scans the Views folders on disk at runtime, so the files are found.
- **Deployed fails:** publish only copies files listed as `<Content>` in the `.csproj`. These three aren't listed, so they never reach the AI Server → "The view 'MemberViewFrame' … was not found."

The three unregistered files:
- `Views\MedicalScrutiny\MemberViewFrame.cshtml`
- `Views\MedicalScrutiny\ProviderDetailsFrame.cshtml`
- `Views\Shared\_ClaimAIFrameLayout.cshtml` (the shared layout those frames use — also unregistered, so it must be added too)

## Fix — Option A: Visual Studio (easiest)
1. In **Solution Explorer**, click **Show All Files** (the toolbar icon at the top of Solution Explorer).
2. The three files above will appear **faded/dotted** (on disk but not in the project).
3. Right-click each one → **Include In Project**.
4. Click each, and in **Properties** confirm **Build Action = Content**.
5. **Rebuild**, then **republish** to the AI Server.

That's usually all it takes. Option B is the exact same result via the file, if you prefer to see the diff.

## Fix — Option B: edit the .csproj directly

### Edit 1 — the two MedicalScrutiny frames

BEFORE (around line 1863):

```xml
    <Content Include="Views\MedicalScrutiny\ClaimsView.cshtml" />
    <Content Include="Views\MedicalScrutiny\Index.cshtml" />
```

AFTER:

```xml
    <Content Include="Views\MedicalScrutiny\ClaimsView.cshtml" />
    <Content Include="Views\MedicalScrutiny\Index.cshtml" />
    <Content Include="Views\MedicalScrutiny\MemberViewFrame.cshtml" />
    <Content Include="Views\MedicalScrutiny\ProviderDetailsFrame.cshtml" />
```

### Edit 2 — the shared ClaimAI frame layout

BEFORE (around line 1912):

```xml
    <Content Include="Views\Shared\_ClaimProviderDetails.cshtml" />
```

AFTER:

```xml
    <Content Include="Views\Shared\_ClaimAIFrameLayout.cshtml" />
    <Content Include="Views\Shared\_ClaimProviderDetails.cshtml" />
```

## After applying
1. **Rebuild Solution** (so the project reloads with the new Content items).
2. **Republish/redeploy** to the AI Server — this time the publish output will include the three views.
3. **Recycle the app pool** on the AI Server so the new files are picked up.
4. Open a claim → **Member View** and **Provider Details** tabs should load.

## Important — commit this to your branch/PR
Because it worked locally, this gap is easy to miss, but it will break on **every** deploy until the `.csproj` is fixed. Make sure this `.csproj` change is committed to your feature branch and included in the Bitbucket PR — otherwise the next person who publishes hits the same error. (This is also a good thing to check for the other new files in your merge: if any other newly-added `.cshtml` / `.js` / static file isn't registered as `Content`, it'll be missing after publish too.)
