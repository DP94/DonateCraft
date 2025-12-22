terraform {
  required_providers {
    aws = {
      source  = "hashicorp/aws"
      version = "6.17.0"
    }
  }
  required_version = ">= 1.1.0"

  backend "s3" {
    bucket = "donatecraft-terraform"
    region = "eu-west-2"
  }
}

provider "aws" {
  region = var.aws_region
}