import { OsuUserStatus, OsuApiErrorStatus, OsuApiSuccessStatus } from "../../utils/common";

/**
 * osu! API token data interface.
 */
interface IOsuApiTokenData {
  token: string;
  expire: Date;
}

/**
 * osu! API user data interface.
 */
interface IOsuApiUserData {
  status: OsuUserStatus;
  user?: {
    userName: string;
    country: string;
  };
}

/**
 * osu! API token response interface.
 */
interface IOsuApiTokenResponseData {
  access_token: string;
  expires_in: number;
  token_type: string;
}

/**
 * osu! API user response interface.
 */
interface IOsuApiUserResponseData {
  avatar_url: string;
  country_code: string;
  cover_url: string;
  default_group: string;
  discord?: string;
  has_supported: boolean;
  id: number;
  interests?: string;
  is_active: boolean;
  is_bot: boolean;
  is_deleted: boolean;
  is_online: boolean;
  is_supporter: boolean;
  join_date: Date | string;
  kudosu: {
    available: number;
    total: number;
  };
  last_visit?: Date | null;
  location?: string;
  max_blocks: number;
  max_friends: number;
  occupation?: string;
  playmode: unknown; // GameMode structure, not interested for now
  playstyle: string[];
  pm_friends_only: boolean;
  post_count: number;
  profile_colour?: string;
  profile_order: unknown; // ProfilePage[], not interested for now
  title?: string;
  title_url?: string;
  twitter?: string;
  username: string;
  website?: string;
}

/**
 * osu! API success response data generic interface.
 */
interface IOsuApiSuccessResponseData<T> {
  status: OsuApiSuccessStatus.OK;
  data: T;
}

/**
 * osu! API error response data generic interface.
 */
interface IOsuApiErrorResponseData<T extends OsuApiErrorStatus> {
  status: T;
}

/**
 * Main osu! API response data type.
 */
type OsuApiResponseData<T> = T extends OsuApiErrorStatus ? IOsuApiErrorResponseData<T> : IOsuApiSuccessResponseData<T>;

/**
 * Checks whether response's type is `IOsuApiErrorResponseData`.
 *
 * @param { unknown } response Response to be checked.
 *
 * @returns { response is IOsuApiErrorResponseData<OsuApiErrorStatus> } Returns `true` if response is an error, `false` otherwise.
 */
function isOsuApiErrorResponse(response: unknown): response is IOsuApiErrorResponseData<OsuApiErrorStatus> {
  return (response as IOsuApiErrorResponseData<OsuApiErrorStatus>).status !== OsuApiErrorStatus.OK;
}

export { IOsuApiTokenData, IOsuApiUserData, IOsuApiTokenResponseData, IOsuApiUserResponseData, OsuApiResponseData, isOsuApiErrorResponse };
