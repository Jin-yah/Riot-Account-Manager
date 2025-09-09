using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Text;

namespace LoLAccountLauncher.Services
{
    /// <summary>
    /// A wrapper for the Windows Credential Manager API.
    /// </summary>
    public static class CredentialManager
    {
        /// <summary>
        /// Saves a generic credential to the Windows Credential Manager.
        /// </summary>
        /// <param name="target">The target name for the credential.</param>
        /// <param name="user">The username associated with the credential.</param>
        /// <param name="password">The password to be stored.</param>
        public static void SaveCredential(string target, string user, string password)
        {
            byte[] passwordBytes = Encoding.Unicode.GetBytes(password);
            IntPtr passwordPtr = Marshal.AllocHGlobal(passwordBytes.Length);
            try
            {
                Marshal.Copy(passwordBytes, 0, passwordPtr, passwordBytes.Length);
                var cred = new CREDENTIAL
                {
                    Type = CRED_TYPE.GENERIC,
                    TargetName = target,
                    UserName = user,
                    CredentialBlob = passwordPtr,
                    CredentialBlobSize = (uint)passwordBytes.Length,
                    Persist = CRED_PERSIST.LOCAL_MACHINE,
                };

                if (!CredWrite(ref cred, 0))
                    throw new Win32Exception(Marshal.GetLastWin32Error());
            }
            finally
            {
                Marshal.FreeHGlobal(passwordPtr);
            }
        }

        /// <summary>
        /// Retrieves a password from a generic credential in the Windows Credential Manager.
        /// </summary>
        /// <param name="target">The target name of the credential to retrieve.</param>
        /// <returns>The password if found; otherwise, null.</returns>
        public static string? GetCredential(string target)
        {
            if (!CredRead(target, CRED_TYPE.GENERIC, 0, out IntPtr credPtr))
            {
                int error = Marshal.GetLastWin32Error();
                if (error == 1168) // ERROR_NOT_FOUND
                    return null;
                throw new Win32Exception(error);
            }

            var credObj = Marshal.PtrToStructure(credPtr, typeof(CREDENTIAL));
            if (credObj == null)
            {
                CredFree(credPtr);
                return null;
            }
            var cred = (CREDENTIAL)credObj; // Unboxing is safe now
            var password = Marshal.PtrToStringUni(
                cred.CredentialBlob,
                (int)cred.CredentialBlobSize / 2
            );
            CredFree(credPtr);
            return password;
        }

        /// <summary>
        /// Deletes a generic credential from the Windows Credential Manager.
        /// </summary>
        /// <param name="target">The target name of the credential to delete.</param>
        public static void DeleteCredential(string target)
        {
            if (!CredDelete(target, CRED_TYPE.GENERIC, 0))
            {
                int error = Marshal.GetLastWin32Error();
                if (error != 1168) // Ignore ERROR_NOT_FOUND
                    throw new Win32Exception(error);
            }
        }

        #region P/Invoke Structures and Functions

        /// <summary>
        /// Defines the type of a credential.
        /// </summary>
        private enum CRED_TYPE : uint
        {
            GENERIC = 1,
            DOMAIN_PASSWORD = 2,
            DOMAIN_CERTIFICATE = 3,
            DOMAIN_VISIBLE_PASSWORD = 4,
            GENERIC_CERTIFICATE = 5,
            DOMAIN_EXTENDED = 6,
            MAXIMUM = 7,
        }

        /// <summary>
        /// Defines the persistence of a credential.
        /// </summary>
        private enum CRED_PERSIST : uint
        {
            SESSION = 1,
            LOCAL_MACHINE = 2,
            ENTERPRISE = 3,
        }

        /// <summary>
        /// Defines a credential record.
        /// </summary>
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        private struct CREDENTIAL
        {
            public uint Flags;
            public CRED_TYPE Type;
            public string TargetName;
            public string Comment;
            public System.Runtime.InteropServices.ComTypes.FILETIME LastWritten;
            public uint CredentialBlobSize;
            public IntPtr CredentialBlob;
            public CRED_PERSIST Persist;
            public uint AttributeCount;
            public IntPtr Attributes;
            public string TargetAlias;
            public string UserName;
        }

        /// <summary>
        /// Creates a new credential or modifies an existing credential.
        /// </summary>
        [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern bool CredWrite(ref CREDENTIAL credential, uint flags);

        /// <summary>
        /// Reads a credential from the user's credential set.
        /// </summary>
        [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern bool CredRead(
            string target,
            CRED_TYPE type,
            uint flags,
            out IntPtr credential
        );

        /// <summary>
        /// Deletes a credential from the user's credential set.
        /// </summary>
        [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern bool CredDelete(string target, CRED_TYPE type, uint flags);

        /// <summary>
        /// Frees the buffer returned by any of the credential management functions.
        /// </summary>
        [DllImport("advapi32.dll", SetLastError = true)]
        private static extern void CredFree(IntPtr buffer);

        #endregion
    }
}
