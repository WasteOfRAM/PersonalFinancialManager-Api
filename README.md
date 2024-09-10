# PersonalFinancialManager Api
###### Work in progress everything is subject to change. Documentation may be behind and uncomplete at times.  

![Unit Tests](https://github.com/WasteOfRAM/PersonalFinancialManager-Api/actions/workflows/unit-tests.yml/badge.svg)

#### An API for managing personal finances. It is standalone does not connect to any bank or other finance apps.

## Description
Pending ..

## Endpoints

## Users  

### **/api/v1/user/register**

**Method:** POST  
**Headers:**  
&nbsp;&nbsp;&nbsp;&nbsp; Content-Type: application/json

**Body:**  
&nbsp;&nbsp;&nbsp;&nbsp; "email" (string, required)  
&nbsp;&nbsp;&nbsp;&nbsp; "password" (string, required)

```json
{
    "email": "string",
    "password": "string"
}
```

**Response:**  
&nbsp;&nbsp;&nbsp;&nbsp; Status code: 200 OK  

---

### **/api/v1/user/login**

**Method:** POST  
**Headers:**  
&nbsp;&nbsp;&nbsp;&nbsp; Content-Type: application/json

**Body:**  
&nbsp;&nbsp;&nbsp;&nbsp; "email" (string, required)  
&nbsp;&nbsp;&nbsp;&nbsp; "password" (string, required)

```json
{
    "email": "string",
    "password": "string"
}
```

**Response:**  
&nbsp;&nbsp;&nbsp;&nbsp; Status code: 200 OK  

**Body:**   

```json
{
    "accessToken": "string",
    "refreshToken": "string"
}
```

---

### **/api/v1/user/refresh**

**Method:** POST  
**Headers:**  
&nbsp;&nbsp;&nbsp;&nbsp; Content-Type: application/json  
&nbsp;&nbsp;&nbsp;&nbsp; Authorization: Bearer accessToken

**Body:**  
&nbsp;&nbsp;&nbsp;&nbsp; "refreshToken" (string, required)  

```json
{
    "refreshToken": "string"
}
```

**Response:**  
&nbsp;&nbsp;&nbsp;&nbsp; Status code: 200 OK  

**Body:**   

```json
{
    "accessToken": "string",
    "refreshToken": "string"
}
```

---

## Accounts
### **/api/v1/account**

**Method:** POST  
**Headers:**  
&nbsp;&nbsp;&nbsp;&nbsp; Content-Type: application/json  
&nbsp;&nbsp;&nbsp;&nbsp; Authorization: Bearer accessToken

**Body:**  
&nbsp;&nbsp;&nbsp;&nbsp; "name" (string, required)  
&nbsp;&nbsp;&nbsp;&nbsp; "currency" (string, required)  
&nbsp;&nbsp;&nbsp;&nbsp; "accountType" (string, required)  
&nbsp;&nbsp;&nbsp;&nbsp; "total" (number, optional)  
&nbsp;&nbsp;&nbsp;&nbsp; "description" (string, optional)  

```json
{
  "name": "string",
  "currency": "string",
  "accountType": "string",
  "total": 0.0,
  "description": "string"
}
```

**Response:**  
&nbsp;&nbsp;&nbsp;&nbsp; Status code: 200 OK  

**Body:**   
```json
{
  "id": "8ebffcff-7480-4fcd-bdae-08dcc1c648ee",
  "name": "Name",
  "currency": "EUR",
  "accountType": "Cash",
  "creationDate": "21.08.2024",
  "total": 0.0,
  "description": "Description"
}
```

---

### **/api/v1/account/{id}**

**Method:** GET  
**Headers:**  
&nbsp;&nbsp;&nbsp;&nbsp; Authorization: Bearer accessToken
**Path Parameters:**  
&nbsp;&nbsp;&nbsp;&nbsp; "id" (uuid, required)

**Response:**  
&nbsp;&nbsp;&nbsp;&nbsp; Status code: 200 OK  

**Body:**   
```json
{
  "id": "8ebffcff-7480-4fcd-bdae-08dcc1c648ee",
  "name": "Name",
  "currency": "EUR",
  "accountType": "Cash",
  "creationDate": "21.08.2024",
  "total": 0.0,
  "description": "Description"
}
```

---

### **/api/v1/account/{id}/transactions**

**Method:** GET  
**Headers:**  
&nbsp;&nbsp;&nbsp;&nbsp; Authorization: Bearer accessToken  
**Path Parameters:**  
&nbsp;&nbsp;&nbsp;&nbsp; "id" (uuid, required)  

**Query Parameters:**  
&nbsp;&nbsp;&nbsp;&nbsp; "search" (string, optional)  
&nbsp;&nbsp;&nbsp;&nbsp; "order" (string, optional)  
&nbsp;&nbsp;&nbsp;&nbsp; "orderBy" (string, optional)  
&nbsp;&nbsp;&nbsp;&nbsp; "page" (string, optional)  
&nbsp;&nbsp;&nbsp;&nbsp; "itemsPerPage" (string, optional)  

**Response:**  
&nbsp;&nbsp;&nbsp;&nbsp; Status code: 200 OK  

**Body:**   
```json
{
  "id": "8ebffcff-7480-4fcd-bdae-08dcc1c648ee",
  "name": "Name",
  "currency": "EUR",
  "accountType": "Cash",
  "creationDate": "21.08.2024",
  "total": 300.0000,
  "description": null,
  "transactions": {
    "search": null,
    "itemsCount": 2,
    "currentPage": 1,
    "itemsPerPage": null,
    "orderBy": "CreationDate",
    "order": "DESC",
    "items": [
      {
        "id": "0c3b492a-8271-4e16-e193-08dcc1dc67e3",
        "transactionType": "Withdraw",
        "amount": 33.0000,
        "creationDate": "21.08.2024",
        "description": null,
        "accountId": "8ebffcff-7480-4fcd-bdae-08dcc1c648ee"
      },
      {
        "id": "d5bb787a-9c52-4401-07cc-08dcc1c648f1",
        "transactionType": "Deposit",
        "amount": 333.0000,
        "creationDate": "21.08.2024",
        "description": "Automated transaction on account creation.",
        "accountId": "8ebffcff-7480-4fcd-bdae-08dcc1c648ee"
      }
    ]
  }
}
```

---

### **/api/v1/account/**  
**Method:** GET  
**Headers:**  
&nbsp;&nbsp;&nbsp;&nbsp; Authorization: Bearer accessToken 

**Query Parameters:**  
&nbsp;&nbsp;&nbsp;&nbsp; "search" (string, optional)  
&nbsp;&nbsp;&nbsp;&nbsp; "order" (string, optional)  
&nbsp;&nbsp;&nbsp;&nbsp; "orderBy" (string, optional)  
&nbsp;&nbsp;&nbsp;&nbsp; "page" (string, optional)  
&nbsp;&nbsp;&nbsp;&nbsp; "itemsPerPage" (string, optional)  

**Response:**  
&nbsp;&nbsp;&nbsp;&nbsp; Status code: 200 OK  

**Body:**   
```json
{
  "search": null,
  "itemsCount": 2,
  "currentPage": 1,
  "itemsPerPage": null,
  "orderBy": null,
  "order": "ASC",
  "items": [
    {
      "id": "b172def9-22b2-44e9-85b1-08dcb20f70fa",
      "name": "Cash",
      "currency": "EUR",
      "accountType": "Cash",
      "creationDate": "01.08.2024",
      "total": 14.0127,
      "description": "Cash"
    },
    {
      "id": "f8cdfcd9-a9b2-4b57-b211-08dcb21d35b1",
      "name": "MyBank",
      "currency": "BGN",
      "accountType": "Bank",
      "creationDate": "01.08.2024",
      "total": 0.0000,
      "description": null
    }
  ]
}
```

---

### **/api/v1/account/**  

**Method:** PUT    
**Headers:**  
&nbsp;&nbsp;&nbsp;&nbsp; Content-Type: application/json  
&nbsp;&nbsp;&nbsp;&nbsp; Authorization: Bearer accessToken  

**Body:**  
&nbsp;&nbsp;&nbsp;&nbsp; "id" (uuid, required)  
&nbsp;&nbsp;&nbsp;&nbsp; "name" (string, required)  
&nbsp;&nbsp;&nbsp;&nbsp; "currency" (string, required)  
&nbsp;&nbsp;&nbsp;&nbsp; "accountType" (string, required)  
&nbsp;&nbsp;&nbsp;&nbsp; "total" (number, optional)  
&nbsp;&nbsp;&nbsp;&nbsp; "description" (string, optional)  

```json
{
  "id": "string",
  "name": "string",
  "currency": "string",
  "accountType": "string",
  "total": 0.0,
  "description": "string"
}
```

**Response:**  
&nbsp;&nbsp;&nbsp;&nbsp; Status code: 200 OK  

**Body:**   
```json
{
  "id": "ffc91d63-038c-443b-08ed-08dcb5380d72",
  "name": "Mery card",
  "currency": "EUR",
  "accountType": "Card",
  "creationDate": "05.08.2024",
  "total": 115.14,
  "description": "daughter alowens card"
}
```

---

### **/api/v1/account/{id}**  

**Method:** DELETE    
**Headers:**  
&nbsp;&nbsp;&nbsp;&nbsp; Authorization: Bearer accessToken  

**Path Parameters:**  
&nbsp;&nbsp;&nbsp;&nbsp; "id" (uuid, required)  

**Response:**  
&nbsp;&nbsp;&nbsp;&nbsp; Status code: 204 No Content  

---

## Transactions

### **/api/v1/transaction/**  

**Method:** POST    
**Headers:**  
&nbsp;&nbsp;&nbsp;&nbsp; Content-Type: application/json  
&nbsp;&nbsp;&nbsp;&nbsp; Authorization: Bearer accessToken  

**Body:**  
&nbsp;&nbsp;&nbsp;&nbsp; "accountId" (string, required)  
&nbsp;&nbsp;&nbsp;&nbsp; "transactionType" (string, required)  
&nbsp;&nbsp;&nbsp;&nbsp; "amount" (number, required)  
&nbsp;&nbsp;&nbsp;&nbsp; "description" (string, optional)  

```json
{
  "accountId": "string",
  "transactionType": "string",
  "amount": 1,
  "description": "string"
}
```

**Response:**  
&nbsp;&nbsp;&nbsp;&nbsp; Status code: 200 OK  

**Body:**   
```json
{
  "id": "d354a2fc-d7e9-4ff3-e194-08dcc1dc67e3",
  "transactionType": "Withdraw",
  "amount": 33,
  "creationDate": "21.08.2024",
  "description": "Dounuts and coffe.",
  "accountId": "8ebffcff-7480-4fcd-bdae-08dcc1c648ee"
}
```

---

### **/api/v1/transaction/{id}**  

**Method:** GET    
**Headers:**  
&nbsp;&nbsp;&nbsp;&nbsp; Authorization: Bearer accessToken  

**Path Parameters:**  
&nbsp;&nbsp;&nbsp;&nbsp; "id" (uuid, required)  

**Response:**  
&nbsp;&nbsp;&nbsp;&nbsp; Status code: 200 OK  

**Body:**   
```json
{
  "id": "d354a2fc-d7e9-4ff3-e194-08dcc1dc67e3",
  "transactionType": "Withdraw",
  "amount": 33,
  "creationDate": "21.08.2024",
  "description": "Dounuts and coffe.",
  "accountId": "8ebffcff-7480-4fcd-bdae-08dcc1c648ee"
}
```

---

### **/api/v1/transaction/** 

**Method:** GET    
**Headers:**  
&nbsp;&nbsp;&nbsp;&nbsp; Authorization: Bearer accessToken  

**Query Parameters:**  
&nbsp;&nbsp;&nbsp;&nbsp; "search" (string, optional)  
&nbsp;&nbsp;&nbsp;&nbsp; "order" (string, optional)  
&nbsp;&nbsp;&nbsp;&nbsp; "orderBy" (string, optional)  
&nbsp;&nbsp;&nbsp;&nbsp; "page" (string, optional)  
&nbsp;&nbsp;&nbsp;&nbsp; "itemsPerPage" (string, optional)  

**Response:**  
&nbsp;&nbsp;&nbsp;&nbsp; Status code: 200 OK  

**Body:**   
```json
{
  "search": null,
  "itemsCount": 20,
  "currentPage": 1,
  "itemsPerPage": null,
  "orderBy": "CreationDate",
  "order": "DESC",
  "items": [
    {
      "id": "343687de-f755-4298-161c-08dcbd235294",
      "transactionType": "Deposit",
      "amount": 12,
      "creationDate": "21.08.2024",
      "description": null,
      "accountId": "8ebffcff-7480-4fcd-bdae-08dcc1c648ee"
    },
    {
      "id": "d354a2fc-d7e9-4ff3-e194-08dcc1dc67e3",
      "transactionType": "Withdraw",
      "amount": 33,
      "creationDate": "21.08.2024",
      "description": "Dounuts and coffe.",
      "accountId": "8ebffcff-7480-4fcd-bdae-08dcc1c648ee"
    }
  ]
}
```

---

### **/api/v1/transaction/** 

**Method:** PUT    
**Headers:**  
&nbsp;&nbsp;&nbsp;&nbsp; Content-Type: application/json  
&nbsp;&nbsp;&nbsp;&nbsp; Authorization: Bearer accessToken  

**Body:**  
&nbsp;&nbsp;&nbsp;&nbsp; "id" (uuid, required)  
&nbsp;&nbsp;&nbsp;&nbsp; "accountId" (uuid, required)  
&nbsp;&nbsp;&nbsp;&nbsp; "transactionType" (string, required)  
&nbsp;&nbsp;&nbsp;&nbsp; "total" (number, required)  
&nbsp;&nbsp;&nbsp;&nbsp; "description" (string, optional)  

```json
{
  "id": "17427333-1d70-444b-e674-08dcbc5bd621",
  "accountId": "33a170ba-425b-4aef-6faa-08dcbc5ba9d0",
  "transactionType": "Withdraw",
  "amount": 5,
  "description": null
}
```

**Response:**  
&nbsp;&nbsp;&nbsp;&nbsp; Status code: 200 OK  

**Body:**   
```json
    {
      "id": "d354a2fc-d7e9-4ff3-e194-08dcc1dc67e3",
      "transactionType": "Withdraw",
      "amount": 15,
      "creationDate": "21.08.2024",
      "description": null,
      "accountId": "8ebffcff-7480-4fcd-bdae-08dcc1c648ee"
    }
```

---

### **/api/v1/transaction/{id}** 

**Method:** DELETE    
**Headers:**  
&nbsp;&nbsp;&nbsp;&nbsp; Authorization: Bearer accessToken  

**Path Parameters:**  
&nbsp;&nbsp;&nbsp;&nbsp; "id" (uuid, required)  

**Response:**  
&nbsp;&nbsp;&nbsp;&nbsp; Status code: 204 No Content  
