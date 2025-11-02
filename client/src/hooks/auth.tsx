import { useNavigate } from "react-router-dom";
import { authClient } from "../api-clients";
import type { AuthUserInfo, LoginRequest } from "../models/generated-client";
import { useAtom } from "jotai";
import { tokenAtom, userInfoAtom } from "../atoms/token";

type AuthHook = {
  user: AuthUserInfo | null;
  login: (request: LoginRequest) => Promise<void>;
  logout: () => void;
};

export const useAuth = () => {
  const [_, setJwt] = useAtom(tokenAtom);
  const [user] = useAtom(userInfoAtom);
  const navigate = useNavigate();

  const login = async (request: LoginRequest) => {
    const response = await authClient.login(request);
    setJwt(response.jwt!);
    navigate("/");
  };

  const logout = async () => {
    setJwt(null);
    navigate("/login");
  };

  return {
    user,
    login,
    logout,
  } as AuthHook;
};
