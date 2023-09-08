# Cool-Shop

## Introduction
This project was completed as part of Codecool Academy. Our task was to create an online store page with user login and registration functionality, adding products to the cart, and generating order confirmation during two weekly sprints. At the end of each sprint we created a presentation and presented our application in front of the whole group.

## Task Sprints

### Sprint 1:
At the outset, we discussed what we wanted to create and how to do it. Then, we wrote the tasks we needed to complete and estimated the time required to achieve the MVP of our product. We designed the architecture and application models. Afterward, we configured a Microsoft SQL Server database using Entity Framework, began building the foundational components for the shopping cart and the order processing system.

### Sprint 2:
We integrated the Identity Framework into our application and implemented event logging using Serilog. Subsequently, we finalized the 'add to cart' functionality and the ordering process. We created a data seeder using the Bogus library and, for the first time, began introducing unit tests into our applications.

## About project
Cool-Shop is an online store where we can search for products, add them to the basket, place an order, and simulate a payment. We have implemented user registration, Serilog for logging, a database, and used Bogus library for generating fictitious products.

![Cool-Shop](https://github.com/MarcinSzkurlat/Cool-Shop/assets/94744112/6e882048-d725-4aa0-aac9-668285d260cf)


## Getting Started

Clone this repo.
```
git clone https://github.com/MarcinSzkurlat/Cool-Shop.git
```

Make sure you have installed Docker on your computer. After that, you can run the below command from the /src/ directory and get started with the Cool-Shop immediately.
```gitbash
docker compose up
```

You should be able to browse the application by using the below URL:
```
http://localhost:5092
```
