docker build -t e-stock-api .

heroku login
heroku container:login

docker tag e-stock-api registry.heroku.com/e-stock-api/web
docker push registry.heroku.com/e-stock-api/web

heroku container:release web -a e-stock-api