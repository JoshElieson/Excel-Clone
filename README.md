# Spreadsheet Application

A modern spreadsheet application inspired by Microsoft Excel, originally developed in a team environment for CS3500 (Software Practice) at the University of Utah, and later rebuilt as a full-stack web application for deployment and portfolio presentation.

## Overview

This project began as a desktop/mobile spreadsheet application built using:

- .NET MAUI
- C#
- XAML

The original implementation focused on software architecture, UI/UX design, formula parsing, dependency management, and collaborative software development practices.

I later rebuilt the project using a modern web stack with a React frontend and deployed it publicly through Vercel to showcase the project in a more accessible and interactive format.

## Features

- Excel-like spreadsheet interface
- Cell-based editing system
- Formula parsing and evaluation
- Dependency graph support
- Automatic cell recalculation
- Dynamic expression updates
- Error handling for invalid formulas
- Persistent spreadsheet state
- Responsive desktop/web interface

## Formula Support

The spreadsheet supports mathematical expressions and inter-cell dependencies similar to Excel, including:

```txt
=A1 + B2
=C5 * 2
=(A1 + A2) / B1
