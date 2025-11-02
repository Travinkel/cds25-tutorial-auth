import { TOKEN_KEY, tokenStorage } from "./atoms/token";
import { AuthClient, BlogClient, DraftClient } from "./models/generated-client";

const customFetch = async (url: RequestInfo, init?: RequestInit) => {
    const token = tokenStorage.getItem(TOKEN_KEY, null);
    if (token) {
        init = {
            ...(init ?? {}),
            headers: {
                ...(init?.headers ?? {}),
                Authorization: `Bearer ${token}`,
            },
        };
    }
    return fetch(url, init);
};

// Use VITE_API_BASE if set, otherwise derive API host from current origin
const API_BASE = (import.meta.env.VITE_API_BASE as string | undefined)
    ?? window.location.origin.replace("5173", "5153");

export const authClient = new AuthClient(API_BASE, { fetch: customFetch });
export const blogClient = new BlogClient(API_BASE, { fetch: customFetch });
export const draftClient = new DraftClient(API_BASE, { fetch: customFetch });
