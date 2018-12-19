import { User } from './user';
import { Contact } from './contact';

export class ResponseData {
  user: User;
  contact: Contact;
  contacts: Contact[];
}

export class PayloadResponse {
  code: number;
  message: string;
  data: ResponseData;
}
