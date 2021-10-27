ARG PY_BASE_IMG

FROM python:${PY_BASE_IMG}

WORKDIR /app

RUN apk add --update --no-cache gcc python3-dev build-base libev-dev libffi-dev bash

COPY requirements.txt /app
RUN pip3 install -r requirements.txt

COPY . /app

ENV PYTHONUNBUFFERED 1
ENTRYPOINT ["python3"]
CMD ["server.py"]
