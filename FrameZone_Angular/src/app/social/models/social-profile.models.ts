export interface SocialProfileSummary {
  userId: number;
  displayName: string;
  avatar?: string | null;
  followingCount: number;
  followerCount: number;
}
