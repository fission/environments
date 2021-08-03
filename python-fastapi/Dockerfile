FROM python:3.7-alpine

WORKDIR /app

RUN pip install --no-cache-dir --upgrade pip

COPY requirements.txt /app
RUN pip install --no-cache-dir --upgrade -r requirements.txt

COPY . /app

ENV PYTHONUNBUFFERED 1
ENTRYPOINT ["python"]
CMD ["server.py"]
