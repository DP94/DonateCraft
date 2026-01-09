terraform {
  required_providers {
    aws = {
      source  = "hashicorp/aws"
      version = "6.27.0"
    }
    awscc = {
      source  = "hashicorp/awscc"
      version = "1.60.0"
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