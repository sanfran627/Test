export class Contact {
  contactId: string;
  userId: string
  firstName: string;
  lastName: string;
  dob: Date;
  age: number;
  addresses: Map<AddressType, Address>
}

export enum AddressType {
  Home = 0,
  Work = 1
}

export class Address {
  address1: string;
  address2: string;
  city: string;
  state: string;
  country: string;
  postal: string;
}
