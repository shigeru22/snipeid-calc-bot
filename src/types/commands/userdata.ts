import { OsuUserStatus } from "../../utils/common";

/**
 * osu! user data interface.
 */
interface IOsuUserData {
  status: OsuUserStatus.USER;
  userName: string;
  country: string;
}

export { IOsuUserData };
