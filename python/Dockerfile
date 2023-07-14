ARG PY_BASE_IMG

FROM python:${PY_BASE_IMG}

WORKDIR /app

RUN apk add --update --no-cache gcc python3-dev build-base libev-dev libffi-dev bash

COPY requirements.txt /app
RUN pip3 install --no-cache-dir -r requirements.txt

ENV WSGI_FRAMEWORK GEVENT

COPY *.py /app

ENV PYTHONUNBUFFERED 1

CMD [ "gunicorn", "--bind=0.0.0.0:8888", "--timeout", "900", "-k", "gevent", "-w", "2", "server:app" ] 
