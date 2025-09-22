export type User = {
  id: string;
  displayName: string;
  email: string;
  token: string;
  imageUrl?: string;
}

export type LoginCreds = {
    email: string;
    password: string;
}


export type RegisterCred = {
    email: string;
    displayName: string;
    password: string;
}