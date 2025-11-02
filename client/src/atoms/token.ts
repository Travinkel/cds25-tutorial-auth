import { atom } from "jotai";
import { atomWithStorage, createJSONStorage } from "jotai/utils";
import { authClient } from "../api-clients";

// Storage key for JWT
export const TOKEN_KEY = "token";

export const tokenStorage = createJSONStorage<string | null>(
  () => sessionStorage
);

export const tokenAtom = atomWithStorage<string | null>(
  TOKEN_KEY,
  null,
  tokenStorage
);

export const userInfoAtom = atom(async (get) => {
  const token = get(tokenAtom);
  if (!token) return null;

  try {
    const userInfo = await authClient.userInfo();
    return userInfo;
  } catch (err: any) {
    // ✅ Handle unauthenticated users gracefully
    if (err.status === 401) {
      console.warn("No valid session — user is anonymous.");
      return null;
    }
    console.error("Unexpected error fetching user info:", err);
    throw err; // only rethrow non-401 errors
  }
});


