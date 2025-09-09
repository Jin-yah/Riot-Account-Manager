# LoL Account Launcher

A lightweight Windows desktop application for managing and quickly logging into multiple League of Legends accounts.

This app provides a simple interface to save account credentials securely and launch the League of Legends client with a single click, bypassing the need to manually enter your username and password.

---

## Features

- **Multi-Account Management**: Save and organize multiple League of Legends accounts.
- **One-Click Login**: Launch the League client and log in automatically.
- **Secure Credential Storage**: Passwords are not stored by the application directly. Instead, they are saved securely in the **Windows Credential Manager**.
- **Drag-and-Drop Reordering**: Easily reorder your account list to prioritize your most-used accounts.

---

## Requirements

- Windows 10/11
- [.NET 8 Desktop Runtime](https://dotnet.microsoft.com/en-us/download/dotnet/8.0)

Alternatively, the project can be published as a self-contained executable that includes the runtime and has no external dependencies.

---

## Usage

1.  **Add an Account**:
    -   Click the "Add Account" button.
    -   Enter the username and password.
    -   Click "Save".
2.  **Log In**:
    -   Click anywhere on the account item in the list. The launcher will attempt to log you into the League of Legends client.
3.  **Edit/Delete**:
    -   Hover over an account to reveal the "Edit" and "Delete" buttons.
4.  **Reorder**:
    -   Click and drag the ":" handle on the left of an account item to change its position in the list.

---

## Data Storage

-   **Account Information**: Usernames and display names are stored in a local SQLite database located at:
    ```
    %APPDATA%\\LoLAccountLauncher\\accounts.db
    ```
-   **Passwords**: Passwords are saved as generic credentials in the Windows Credential Manager under a name like `LoLAccountLauncher_<GUID>`.

---

## License

MIT License — feel free to use, modify, and distribute.

---

## Disclaimer

LoL Account Launcher is not endorsed by Riot Games and does not reflect the views or opinions of Riot Games or anyone officially involved in producing or managing Riot Games properties. Riot Games and all associated properties are trademarks or registered trademarks of Riot Games, Inc